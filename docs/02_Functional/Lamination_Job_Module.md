# Lamination / Cut-to-Length Job Planning

The Lamination Job is the production lot; there is no separate Lamination Batch entity. A job records the customer/OEM references, Grade-derived material specification, Simple or Step-Lap schedule, and Side, Center, Top and Bottom Plate requirements.

The workflow delivered in B4.2A is Draft → Allocated → Released, with cancellation before production starts. Structural schedule changes lock after allocation confirmation. Cutting execution, actual quantities, scrap and the printable Lamination Job Card are deferred.

There is no Drawing Master in MVP. Work Order linkage is optional and deferred. Plate angles are driven by Plate Type and are never planner-editable.

## Revised MVP Lamination Workflow

The supported workflow is **Draft → Released → Material Allocated → Completed**. Draft schedules may be edited, deleted, released, or cancelled. Release approves and freezes the cutting schedule without reserving or deducting Slit Coil inventory. Only Released jobs may create manual Slit Coil reservations, and confirmation is blocked until every width requirement has zero shortage. Completion is allowed only from Allocated and deducts actual consumed weight, releases unused reservation, and retains the original Slit Coil number. Completed and Cancelled jobs are final and read-only. Start Cutting and In Progress are not available for new Lamination Job workflow actions. The completed Lamination Job is the final production-lot record; no Lamination Batch or individual plate inventory records are created.
## Dedicated Material Allocation Workspace

The Lamination-specific workspace is available at `/lamination-jobs/:id/material-allocation` after Release. Requirements are generated from the released Step Schedule, grouped by required width, and retain a plate/step breakdown for planning. Planners manually select one or more eligible Slit Coils, may reserve partial weights, and can edit or release active reservations while the job remains Released. Each saved allocation is immediately an active reservation and therefore reduces Available Weight for concurrent planners.

Confirmation requires every width requirement to have zero shortage and changes the job to Allocated. Confirming does not deduct physical Slit Coil weight; actual consumption is deducted only when the Allocated Lamination Job is completed, and unused reservation is returned. Allocated, Completed, and Cancelled allocation views are read-only. Automatic allocation remains deferred.