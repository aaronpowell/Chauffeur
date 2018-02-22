---
id: deliverable-package
title: Package Deliverable
---

## Package

_Aliases: p, pkg_

    umbraco> package <...filename>

This deliverable will import one or more packages from their XML files that live within your Chauffeur folder. You don't need to provide the file extension, it will add the `.xml` extension for you. This uses the Umbraco packaging engine so you can import pretty much anything that you can import from the back office*.

*Things that are not supported will shown to you in the output.

If you want to import a package from a different location you can provide a `-f:<path>` argument to it. This is only recommended if you're an advanced user though, because you can break things pretty badly on other peoples machines 😉.