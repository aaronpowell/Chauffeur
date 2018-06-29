---
id: testing-tools
title: Chauffeur Testing Tools
---

When it comes to integration testing Umbraco it can be a little difficult, if you want to work with the Umbraco API's you need to enure that Umbraco is running. Traditionally this is hard because you'd have to run the CMS then run the tests within the CMS, making reporting hard.

Because Chauffeur is designed to work with Umbraco API's without the website running it opens up an opportunity to use Chauffeur to orchestrate integration tests.

## `Chauffeur.TestingTools`

To make it easier to develop integration tests Chauffeur has a companion NuGet package called [`Chauffeur.TestingTools`](https://nuget.org/packages/chauffeur.testingtools).

## Creating Umbraco integration tests with Chauffeur.TestingTools

So what do you get with this package? Right now you get an abstract class called `UmbracoHostTestBase` which you inherit from to:

1. Set up a location for the SQL CE database that is unique to that test run
2. Starts Chauffeur with fake input and output streams that you can interact with

Let's look at a basic test:

```csharp
public class HelpTests : UmbracoHostTestBase
{
    [Fact]
    public async Task Help_Will_Be_Successful()
    {
        var result = await Host.Run(new[] { "help" });

        Assert.Equal(DeliverableResponse.Continue, result);
    }
}
```

That. Is. All.

The `UmbracoHostTestBase` uses its constructor to setup everything because that's how [xunit works](https://xunit.github.io/docs/shared-context.html#constructor). Now I don't dictate that you use xunit, if you use anything else I'd love to know how you go and if it doesn't work so we can work together to make it compatible.

The class then exposes the Chauffeur "Host" as a `Host` property that you can execute Chauffeur commands against.

### Working with IO

Let's say you want to read the output of your deliverable? Easy, there's a `TextWriter` property that uses the `MockTextWriter` class I ship and it exposes all the messages written by `WriteLineAsync` as a collection.

What if you want to simulate user input (reading from the console)? Well that's covered too, the `TextReader` property exposes an instance of `MockTextReader` that you can call the `AddCommand` function to add user input to a stack. Be aware that it's read in FIFO mode, so you'll need to order your "reads" correctly.

### Gotcha's when doing Umbraco integration tests

So there's a few things that you **need** to do that are manual steps (or at least, manual at the moment):

**You'll need the Umbraco config files.**

That stuff that lives in `/config`? Yeah you'll need to copy those into your Integration Test project and then set them to be copied to the build output (Properties -> Copy to Output Directory), since Umbraco's internal API's will try and read those files.

This also means you kind of need a web.config, _kind of_. You don't need the full Umbraco web.config, just the `configSections` definition for `umbracoConfiguration`, the `umbracoConfiguration` (pointing to the right config files), a connection string (SQL CE does work!), the `DbProviderFactories` and `membership`.

Check out my app.config in the integration tests of Chauffeur for an example of how it all works.

### Using SQL CE

SQL CE works nicely for integration tests, it's how I do Chauffeur's integration tests, and one of the things the base class does is setup a unique directory for the `Umbraco.sdf` file, so you don't have clashes across multiple tests. But there is a manual step, you need to copy the `amd64` and `x86` directories from the `UmbracoCms` NuGet package (they are in the `UmbracoFiles/bin` folder) into your `bin/Debug` (or whatever the output folder of your tests is). If you don't do this you'll get a really obscure error message, and I can't work out a simpler way to do it than manual copy/paste.

### Working with the Umbraco Services

It's all good that you can run Chauffeur deliverables but what if you're doing something with just plain Umbraco services, maybe reading DocTypes, creating content, etc.?

Again, I got you covered! Since the base class basically starts Umbraco you have access to all Umbraco services off their singletons. Here's a really basic test that ensures you get all the standard data type definitions on install:

```csharp
public class NonChauffeurTests : UmbracoHostTestBase
{
    [Fact]
    public async Task All_Data_Types_Installed()
    {
        // respond with "yes" to install SQL CE
        TextReader.AddCommand("y");
        var result = await Host.Run(new[] { "install" });

        var dataTypeDefinitions = ApplicationContext.Current.Services.DataTypeService.GetAllDataTypeDefinitions();

        Assert.Equal(24, dataTypeDefinitions.Count());
    }
}
```
### Helper Methods

There are a couple of helper methods exposed to make it easier to write your tests and interact with Chauffeur.

#### Installing Umbraco

You can run the `install` deliverable, but to make it easier there's an `InstallUmbraco` method on the base class. This runs the default install using Chauffeur.

```csharp
public class NonChauffeurTests : UmbracoHostTestBase
{
    [Fact]
    public async Task All_Data_Types_Installed()
    {
        await InstallUmbraco();

        var dataTypeDefinitions = ApplicationContext.Current.Services.DataTypeService.GetAllDataTypeDefinitions();

        Assert.Equal(24, dataTypeDefinitions.Count());
    }
}
```

#### Accessing the Chauffeur directory

If you're doing something like setting up a package that is to be imported using the [`package` deliverable](deliverable-package.md). Because of this you'll need the `Chauffeur` folder. When running in an integration tests this is dynamically generated to avoid clashes, but you can get the directory by using the `GetChauffeurFolder` method on the base class.
