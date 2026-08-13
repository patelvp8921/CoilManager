# Sales Order Module

A Sales Order represents **customer demand**: who ordered, the requested products and specifications, quantity and unit, delivery dates, and commercial information. A Work Order represents **fulfilment planning**. Production Jobs represent **shop-floor execution**.

Sales Order confirmation does not reserve inventory, create Work Orders, or create Slitting/Lamination Jobs. Mixed inventory and production fulfilment will be introduced through Work Orders in Sprint S2.

## Numbering and lifecycle

Numbers use SO/{YEAR}/{00001} and reset annually. The server validates/persists the final unique number. S1.1 permits Draft → Confirmed/Cancelled, Confirmed → On Hold/Cancelled, and On Hold → Confirmed/Cancelled. Fulfilment statuses exist for future integrations and cannot be selected manually.

## Lines

Lines support Mother Coil, Slit Coil, and Lamination demand. Core Frame Assembly is reserved. Quantities may use kg, pieces, or sets; summaries keep these units separate. Grade selection reloads thickness, category, and core loss from Grade Master on the server. Client-derived values are never trusted.

Mother/Slit Coil lines require grade and width and use kg. Lamination lines require grade, transformer rating, and either Drawing Number or OEM Job Number, using pieces or sets. Detailed plate/step schedules belong to Work Order and Lamination planning.

Confirmed orders are locked from deletion. Cancellation requires a reason and makes the order read-only.
