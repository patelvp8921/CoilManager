# Work Order to Dispatch and Packing Slip Workflow

## Business flow

`Ready` means material is available for shipment; it does not mean the Work Order is complete. A dispatcher creates a Draft Dispatch, adds delivery, transport, weight, and package details, and then confirms it.

- A Draft Dispatch does not change inventory or Work Order quantities.
- Confirming a Dispatch is transactional: it consumes reserved Work Order allocations, reduces the source coil's available weight, writes an inventory ledger entry, and records coil traceability on the Dispatch.
- A partial confirmation moves the Work Order to `PartiallyDispatched`.
- The final confirmation moves the Work Order to `Completed` and completes its Dispatch operation.
- Confirmed Dispatches are immutable. Only Draft Dispatches may be edited or cancelled.
- Multiple Dispatches are supported, but their confirmed total cannot exceed the Work Order requirement or ready quantity.

## Numbering

Dispatch and packing-slip numbers are assigned by the API inside a serializable database transaction:

- `DSP/{year}/{sequence}`
- `PS/{year}/{sequence}`

Both values have unique database indexes. Work Order numbering remains independently unique.

## Main API endpoints

- `GET /api/work-orders/{id}/dispatch-summary`
- `GET /api/work-orders/{id}/dispatches`
- `POST /api/work-orders/{id}/dispatches`
- `GET|PUT /api/dispatches/{id}`
- `POST /api/dispatches/{id}/confirm`
- `POST /api/dispatches/{id}/cancel`
- `GET /api/dispatches/{id}/packing-slip`
- `GET /api/dispatches/{id}/packing-slip/pdf`

## Permissions

The workflow uses `Dispatch.View`, `Dispatch.Create`, `Dispatch.Edit`, `Dispatch.Confirm`, `Dispatch.Cancel`, and `Dispatch.PrintPackingSlip`. Assign only the actions each operational role needs.

## Operational notes

Apply migration `20260813030648_AddDispatchAndPackingSlipWorkflow` before deploying the API. Confirmation uses optimistic concurrency plus a serializable transaction. If stock, allocation, or Dispatch state changed after the page was loaded, the transaction is rejected rather than partially posting inventory.

Packing slips can be previewed while Draft and are visibly marked `DRAFT`. Confirmed packing slips remain available from the Dispatch detail and list screens.
