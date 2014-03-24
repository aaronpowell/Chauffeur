# Chauffeur

Welcome to Chauffeur, a tool to help you deliver changes to an Umbraco instance.

## Who is Chauffeur?

Chauffeur is a CLI for Umbraco, it will sit with your Umbraco websites `bin` folder and give you an interface to which you can execute commands, known as **Deliverables**, against your installed Umbraco instance.

Chauffeur is developed for Umbraco 7.x as it is designed around the new Umbraco API.

# Getting Started

To get started install Chauffeur and open up a command window and launch `Chauffeur.Runner.exe`, which is the entry point for Chauffeur. From the prompt you can get started running deliverables.

## Included Deliverables

### Help

_Aliases: h_

    umbraco> help

Lists all deliverables installed.

### Quit

_Aliases: q_

    umbraco> quit

Exists Chauffeur

### Content-Type

_Aliases: ct_

    umbraco> content-type <delivery> <arguments>

A series of sub-deliveries which can be undertaken against Umbraco ContentType's.

    umbraco> content-type get-all

Lists all ContentType's in a quick summary.

    umbraco> content-type get <id or alias>

Gets a specific ContentType and lists its properties.

    umbraco> content-type export <id or alias>

Exports a specific ContentType which is then output to Chauffeur's directory.

    umbraco> content-type import <exported file name>

Imports a ContentType from the specified file, you don't need to provide the `.xml` extension it is assumed.

# FAQ

* Why do I see all this extra text when I run `content-type` commands?

> This is a fun bug in 7.0.4, they included some `Console.WriteLine` statements in the source code which Chauffeur ends up receiving. This seems to have been resolved in 7.1.0.

* Can I make my own Deliverables?

> Sure you can, you need to implement the `Deliverable` class and drop your assembly in the `bin` folder and it'll be loaded up.

* What doesn't work?

> Keep in mind that this is running outside of the web context so there's no HttpContext. This means that some Umbraco API's simply won't work, things like the publishing. Also be  aware of the pointy edges of the Umbraco API, there's a lot of `internal` classes and members that Umbraco will expect (have a look at the PropertyGroupId setup) so things might not be setup that you'd expect. Remember we're using the Umbraco APIs but we're bypassing the Umbraco "boot" process. At the moment you have to manage your own dependency chain, and if there are statics these can bleed across Deliverables so be aware.