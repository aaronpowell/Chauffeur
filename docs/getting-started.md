# Getting Started

The easiest way to add Chauffeur to your website is by installing the two NuGet packages into your Umbraco project via Visual Studio:

* [Chauffeur.Runner](https://nuget.org/packages/chuaffeur.runner)
* [Chauffeur](https://nuget.org/packages/chauffeur)

_Note: Installing the `Chauffeur.Runner` package will also install the `Chauffeur` package, as it's a dependency._

Now you've got Chauffeur, it's time to setup your first Delivery. To do that compile your solution (since you just added some NuGet packages!) and open up a console, navigating to where your codebase is on disk.

Next we'll get into Chauffeur:

```ps
PS> ./MyUmbracoSite/bin/Chuaffeur.Runner.exe
```

This will drop you into the Chauffeur application shell and it's time to start working with Deliverables. Since we're setting up we'll use the `scaffold` Deliverable.

```
umbraco> scaffold
```

You'll be prompted for a few questions, whether you want an Install step created and whether your existing site should be exported.

Once that's done you'll have a file (using the file name specified) in your `App_Data\Chauffeur` folder. If you exported your current site you'll also have an XML file which is a package for your site.

Now you're good to go, Chauffeur has prepared everything for you to check into source control and push to others!