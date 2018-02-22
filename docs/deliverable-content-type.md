---
id: deliverable-content-type
title: Content Type Deliverable
---

## Content-Type

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

    umbraco> content-type remove <ids or aliases>

Removes all the content types based on the IDs or Aliases that you provided.

    umbraco> content-type remove-property <content type id or alias> <property alias>

Removes a property from a content type.

