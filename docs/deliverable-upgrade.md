---
id: deliverable-upgrade
title: Upgrade Deliverable
---

# Upgrade

_Aliases:_

Upgrades the Umbraco database from one release to another using the internal Umbraco migration engine. This would ideally be used if you've updated a NuGet package and want to do a non-interactive upgrade process. It will look at the last version installed (according to the database) and compare that to the current version (according to the assemblies) and upgrade to that.

## Usage

    umbraco> upgrade

# Check pending upgrade

Checks if there is an upgrade waiting to be installed.

## Usage

    umbraco> upgrade check