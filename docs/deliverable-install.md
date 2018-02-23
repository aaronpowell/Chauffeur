---
id: deliverable-install
title: Install Deliverable
---

# Install

_Aliases: none_

    umbraco> install

Installs an empty Umbraco database. This is like running the Umbraco installer from the web UI.

While the installer runs it will create the `User` with the the `0` ID, the default user account. This account will have the default username and password as Umbraco provides (which is `admin` and `default`) but you probably won't be able to log in with it as the password is meant to be hashed but the SQL doesn't hash it. You should combine this with the `user` Deliverable to setup the correct user password.

If you're using SqlCE as the database then the Deliverable will prompt tp create the file as well as run the SQL scripts, if you don't want to be prompted pass `y` to the Deliverable when run. If you're using a different database provider make sure there is an empty database to connect to.

**Note: The Deliverable expects the Umbraco connection string to be set in the web.config.**
