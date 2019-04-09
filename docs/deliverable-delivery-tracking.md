---
id: deliverable-delivery-tracking
title: Delivery Tracking Deliverable
---

# Delivery Tracking

_Aliases: <none>_

This deliverable is more for diagnostics about your environment as it can list what Deliveries are available and what ones have been 'signed for' (aka, executed).

### Signed-For

    umbraco> delivery-tracking signed-for

#### Output

    Name               | Date             | Hash
    001-Setup.delivery | 2018-03-07 03:23 | C43A9FECF38471A6BE78ECC5C41DE1BDD55D471A

### Available

    umbraco> delivery-tracking available

#### Output

    Name               | Path
    001-Setup.delivery | C:\_Projects\github\Chauffeur\Chauffeur.Demo\App_Data\Chauffeur\001-Setup.delivery
