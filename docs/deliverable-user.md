---
id: deliverable-user
title: User Deliverable
---

## User

_Aliases: u_

    umbraco> user <delivery> <arguments>

A series of sub-deliveries which can be undertaken against Umbraco users

### Change-Password

    umbraco> user change-password <name> <new password>
    umbraco> user change-password admin password

Changes the password of a user, you generally want to combine this with the `install` Deliverable so you can setup a user that can log in.

### Change-Name

    umbraco> user change-name <username> <new username>
    umbraco> user change-name admin aaron

Changes the user name for a given user.

### Change-LoginName

    umbraco> user change-loginname <login name> <new login name>
    umbraco> user change-loginname admin email@site.com

Changes the login name for a given user.