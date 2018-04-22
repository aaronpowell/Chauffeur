---
id: creating-deliverables
title: Creating Deliverables
---

Creating a Deliverable is really quite simple, you need to do 3 things:

* Create a .NET class that inherits from the `Deliverable` base class
* Have a public constructor
* Have a `DeliverableName` attribute
  * The name must not contain a space

## Creating a class

Let's start creating a basic Deliverable:

```csharp
namespace MyUmbracoSite.Deliverables
{
    [DeliverableName("awesome")]
    public class AwesomeDeliverable : Deliverable
    {
        public AwesomeDeliverable(TextReader reader, TextWriter writer) : base(reader, writer)
        {
        }
    }
}
```

So we've got our name, `awesome` which is what we'll use from Chauffeur to run our Deliverable, and we've got a public constructor that takes two arguments, a `TextReader` and a `TextWriter`. These are bound to public properties on the base class called `In` and `Out` and represent the two IO streams that you may want to interact with.

### Why abstract away the reader and writer?

You might be wondering why you get these two dependencies injected rather than just using `Console.ReaderLine` and `Console.WriteLine`? Well the reason for that is to improve the ability to write unit tests. Chauffeur Deliverables are designed to be tested, and if you're reading/writing to the console it's pretty hard to write tests against them, instead we'll use the raw streams which can be mocked or created yourself.

## Making our Deliverable do something

Now we have our Deliverable, and if you run `Chauffeur.Runner.exe help` it'll appear in the list. But it doesn't do anything, so it's time to make it do something. For that we'll need to override the `Run` method:

```csharp
namespace MyUmbracoSite.Deliverables
{
    [DeliverableName("awesome")]
    public class AwesomeDeliverable : Deliverable
    {
        public AwesomeDeliverable(TextReader reader, TextWriter writer) : base(reader, writer)
        {
        }

        public async override Task<DeliverableResult> Run(string command, string[] args)
        {
            await Out.WriteLineAsync("I'm an awesome deliverable!");
            return DeliverableResult.Continue;
        }
    }
}
```

Congratulations, your Deliverable does something! It's something simple, it writes a message to the output stream (ie: `Console`) and then returns a success result.

The `Run` method that we overrode has two arguments, `command` and `args`:

* `command` this is what the user typed at the command line to get into your deliverable (probably not all that useful, but you can use it to work out if they used an alias or not)
* `args` an array of the space-separated arguments that were provided, use this to pull apart different operations for a Deliverable and arguments to them

We also need to return a state from the Deliverable, there are three results:

- `Shutdown`
- `Continue`
- `FinishedWithError`

Most of the time you either want to return `Continue` or `Shutdown`, `Continue` means you're all good, keep going, `Shutdown` will cause Chauffeur to exit is more useful if you're doing something with a custom host.

## Adding some dependencies

Writing to the output stream is cool and all, but you probably want to do something more useful with it, like interact with Umbraco.

Let's say you want to work with an Umbraco service as as the `ContentTypeService`. Well, Chauffeur can give that to you! Chauffeur has a built-in IoC container that will inject any dependency (that's registered) that you require to your constructor.

```csharp
namespace MyUmbracoSite.Deliverables
{
    [DeliverableName("awesome")]
    public class AwesomeDeliverable : Deliverable
    {
        private readonly IContentTypeService contentTypeService;

        public AwesomeDeliverable(TextReader reader, TextWriter writer, IContentTypeService contentTypeService) : base(reader, writer)
        {
            this.contentTypeService = contentTypeService;
        }

        public async override Task<DeliverableResult> Run(string command, string[] args)
        {
            var contentTypes = contentTypeService.GetAll();

            foreach(var contentType in contentTypes)
                await Out.WriteLineAsync($"Content Type named {contentType.Name} has an alias of {contentType.Alias}");

            return DeliverableResult.Continue;
        }
    }
}
```

Now we've added a new constructor argument, which is an Umbraco service, and then we'll get all the Content Types in our `Run` method, then output their name and alias.

And there you go, you've created your first Deliverable! Next up learn how to [unit test a Deliverable](unit-testing-deliverables.md)