---
id: deliverable-delivery
title: Delivery Deliverable
---

# Delivery

_Aliases: d_

This is the main Deliverable you'll likely use, it's the one that you can run from a Continuous Delivery pipeline. It looks for files that match the `*.delivery` path in the Chauffeur folder and then runs each step within them. Once a file has been run it will be tracked in the Chauffeur tracking table so it won't be run again on that environment.

While you can run this from the Chauffeur shell you're more likely to run it from the CLI.

## Usage

    umbraco> delivery

This will work out what files haven't been executed previously and run them in sequence.

## Anatomy of a Delivery file

A Delivery file is a file that contains a number of Deliverable steps to execute. This an example of one that will setup the database and then change the admin password:

```
## Install the database
install y
## Change the password
user change-password admin mySecretPassword!1
```

You can put comments in the file if you prefix the line with `##`

### Parameterising Delivery files

Sometimes you might want to pass parameters into a Deliverable, for example, the admin user password (you don't want that in source control do you!). To do this you can place a token that starts and ends with `$`, making it look like this:

```
## Install the database
install y
## Change the password
user change-password admin $adminpwd$
```

Now you can pass those into Chauffeur when it starts:

```
PS> Chauffeur.Runner.exe delivery -p:adminpwd=mySecretPassword!1
```

Chauffeur will also inject a couple of parameters for you:

- ChauffeurPath - the Chauffeur directory
- WebsiteRootPath - the root of your Umbraco web application
- UmbracoPath - the path to the Umbraco folder in your web application
- UmbracoVersion - the version of Umbraco
