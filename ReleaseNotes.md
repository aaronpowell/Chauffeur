## New in 0.6.3 (Released 04/09/2015)
* Fixing what burnt me by not writing tests around the last fix. Still no tests, but we'll get there

## New in 0.6.2 (Released 04/09/2015)
* Fixing bug #30 where structure isn't imported

## New in 0.6.1 (Released 03/09/2015)
* Fixing bug #29

## New in 0.6.0 (Released 03/09/2015)
* Rewrite of dependency loading to use a custom `BootManager`
* Refactored some of the deliverable dependencies
* Updated doco on the `content-type` deliverable
* Refactored the `user change-password` to not require the existing password and to use the username for lookup, not id

## New in 0.5.1 (Released 23/06/2015)
* Removed C# 6 usage as AppVoyer doesn't support it
* Turned on warnings as errors

## New in 0.5.0 (Released 23/06/2015)
* Adding some more folders to the settings interface
* Adding new deliverable to show the settings

## New in 0.4.0 (Released 12/06/2015)
* Exposing IoC container pipeline and dependency builder to allow user to register their own dependencies

## New in 0.3.2 (Release 12/06/2014)
* Getting NuGet publishing working so you don't need the CI environment
* Supporting single exported Document Type (#21)
* Fixed bugs with Umbraco 7.2

## New in 0.3.1 (Released 09/06/2014)
* Fixing versioning

## New in 0.3.0 (Released)
* New feature, `delivery`
 * Gives you the ability to create a script that contains multiple Deliverable's to run
* Fixing #15
* Adding `Change Alias` deliverable

### New in 0.2.4 (Released 28/04/2014)
* Fixing issue with SQL CE when the sdf doesn't exist

### New in 0.2.0
* Adding `install` deliverable to setup the database
* Adding `user` deliverable to configure a user
* Adding `package` deliverable to import Data Types, Document Types, Templates and Macros using the Umbraco export format
* Making dependencies injected via a simple IoC container

### New in 0.1.0 (Released 24/03/2014)
* Initial release
 * Example of how Chauffeur could work
 * Implementing `Content-Type` deliverable for inspecting Content Types
 * Implemeneting `Help` deliverable