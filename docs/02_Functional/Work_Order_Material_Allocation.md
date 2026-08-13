# Work Order Material Allocation

Released Work Orders may immediately reserve physical stock only against `PlannedInventoryQuantity`. Mother Coil requirements use Mother Coil inventory; Slit Coil requirements use Slit Coil inventory. Lamination input remains owned by Lamination Job material allocation.

Each allocation is persisted immediately and reduces available inventory. Multiple records and partial reservations are supported. Quantity changes and releases revalidate current physical availability and the remaining Work Order inventory plan inside a transaction.

The dedicated `/work-orders/:id/material-allocation` page shows planned, reserved, remaining, coverage, active reservations, and matching available inventory. Server-generated Next Actions direct planners to allocation while production remains an S2.2B/S2.2C placeholder.