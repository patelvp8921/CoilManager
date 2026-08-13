# MVP Checklist

- [x] Slit Coil inventory and Coil Passport
- [x] Coil Number, Barcode, and QR Code search
- [x] Mother-to-child Coil Traceability
- [x] Inventory transaction history
- [x] Mother Coil generated-child view
- [ ] Camera scanning (deferred)
- [ ] Re-slitting creation (deferred)
- [x] Browser-based single, reprint, and batch Slit Coil Label printing (B3.4)
- [x] Label Version and Print History tracking
- [ ] Direct thermal-printer SDK integration (deferred)
# Sprint 3 — B4.1 Work Order MVP

- [x] Work Order domain, numbering, lifecycle, and product routing
- [x] Manual Mother/Slit Coil weight allocation and availability lookups
- [x] Optional Work Order-linked Slitting Jobs; standalone jobs retained
- [x] Work Order API, Angular planning pages, actions, and dashboard queue
- [x] Core Frame, automatic allocation, dispatch execution, Lamination Jobs deferred
- [x] Customer and Sales Order relationships deferred to optional MVP text fields
# Sprint 3 — B4.2A

- [x] Lamination / Cut-to-Length Job Draft, allocation and release workflow
- [x] Flexible OEM dimensions; no Drawing Master or editable angles
- [x] Manual multi-Slit-Coil reservations and shortage validation
- [x] Optional source drawing attachment
- [x] Angular list, create, detail, allocation and Job Card placeholder routes
- [ ] Auto allocation, cutting execution, scrap and final Job Card (deferred)

# Sprint S1.1 — Customer and Sales Order Foundation

- [x] Customer Master, automatic immutable code, filters, activation, concurrency
- [x] Sales Order annual numbering, Draft editing, confirmation, hold/release, cancellation
- [x] Product-aware Mother Coil, Slit Coil, and Lamination demand lines
- [x] Grade Master-derived thickness, category, and core loss revalidated server-side
- [x] Quantity summaries separated into kg, pieces, and sets
- [x] Sales navigation, list/filter/action pages, single-page editor, detail tabs and deep links
- [x] Permission constants and route metadata without role-name checks
- [x] Confirmation has no inventory reservation, Work Order, or production-job side effects
- [ ] Work Order fulfilment integration (Sprint S2)
- [ ] Inventory reservation, production execution, dispatch allocation, invoicing and tax engine

## Revised MVP Lamination Workflow

The supported workflow is **Draft → Released → Material Allocated → Completed**. Draft schedules may be edited, deleted, released, or cancelled. Release approves and freezes the cutting schedule without reserving or deducting Slit Coil inventory. Only Released jobs may create manual Slit Coil reservations, and confirmation is blocked until every width requirement has zero shortage. Completion is allowed only from Allocated and deducts actual consumed weight, releases unused reservation, and retains the original Slit Coil number. Completed and Cancelled jobs are final and read-only. Start Cutting and In Progress are not available for new Lamination Job workflow actions. The completed Lamination Job is the final production-lot record; no Lamination Batch or individual plate inventory records are created.
## Dedicated Material Allocation Workspace

The Lamination-specific workspace is available at `/lamination-jobs/:id/material-allocation` after Release. Requirements are generated from the released Step Schedule, grouped by required width, and retain a plate/step breakdown for planning. Planners manually select one or more eligible Slit Coils, may reserve partial weights, and can edit or release active reservations while the job remains Released. Each saved allocation is immediately an active reservation and therefore reduces Available Weight for concurrent planners.

Confirmation requires every width requirement to have zero shortage and changes the job to Allocated. Confirming does not deduct physical Slit Coil weight; actual consumption is deducted only when the Allocated Lamination Job is completed, and unused reservation is returned. Allocated, Completed, and Cancelled allocation views are read-only. Automatic allocation remains deferred.
