---
id: deliverable-user
title: User Deliverable
---

## User

_Aliases: u_

    umbraco> user <delivery> <arguments>

A series of sub-deliveries which can be undertaken against Umbraco users

### Create-User

    umbraco> user create-user <name> <login name> <email> <password> <comma separated groups>
    umbraco> user create-user "Aaron Powell" aaronpowell me@email.com password1 group1,group2

Creates a new user and assigns them to the group(s) you specify. Some notes on it:

- If you want a have a multi-part name (so a first name/last name) you'll need to put `"` around it
- You'll have to specify both a login name and email
- The groups are comma separated, so you can set multiple groups at once

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