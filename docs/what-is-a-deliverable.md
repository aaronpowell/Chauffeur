---
id: what-is-a-deliverable
title: What is a Deliverable?
---

Chauffeur is designed for you to run "things" that interact with your Umbraco instance and these "things" are called `Deliverables`.

A Deliverable could be getting the settings of your Umbraco instance, it could be inspecting Content Types, importing packages, or anything you can dream up.

One goal of Chauffeur is to make sure it's highly extensible, so the Deliverables are actually a plugin system. If you want to create your own plugin it'll work just the same as any of the built in ones as they are plugins themselves! even the `quit` Deliverable is a plugin!

If you want to create your own Deliverable [check out this handy guide](creating-deliverables.md).
