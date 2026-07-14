# Work Order Module

Work Orders use `WO-{YEAR}-{00001}` numbering and support Customer Order, Inventory Production, Rework, and Trial demand. The create page displays a provisional next number; the server assigns and uniquely validates the saved number.

Routes are `/work-orders`, `/work-orders/create`, `/work-orders/:id`, `/work-orders/:id/edit`, and `/work-orders/:id/allocations`. API routes are rooted at `/api/work-orders` and return `ApiResponse` or `ApiPagedResponse` envelopes.

Routing is configured from the product:

| Product | Slitting | Lamination | Dispatch |
|---|---|---|---|
| Mother Coil | Not Required | Not Required | Pending |
| Slit Coil | Pending | Not Required | Pending |
| Lamination | Pending by default | Pending | Pending |

A planner may mark Lamination's pending Slitting step Not Required when suitable Slit Coil stock exists. Core Frame Assembly returns a business-rule response. Required operations must be complete before the Work Order can complete.

Slitting Jobs remain standalone by default (`ProductionType = Inventory`). A linked job records Work Order and operation identifiers. Releasing or starting it makes the Slitting operation In Progress; completing it completes the operation.
