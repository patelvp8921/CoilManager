# MVP Business Process Specification

## Work Order planning

A Work Order is the parent production document for Mother Coil supply, Slit Coil production or supply, and Lamination production. Core Frame Assembly is reserved in the product enum but is not executable in this MVP.

Work Orders may represent customer demand or inventory production. Customer Master and Sales Order are deferred; `CustomerName` and `SalesOrderReference` are optional free-text fields until those relationships are introduced.

The controlled lifecycle is Draft → Released → In Production → Completed → Closed. Draft may be cancelled. Released may be cancelled only before an operation starts. Cancelling releases all active material reservations.

Material planning is manual. A planner selects one or more Mother or Slit Coils and reserves a positive portion of each coil's available weight. Automatic allocation, Dispatch execution, Lamination Jobs, and Core Frame Assembly are outside the MVP.

## Revised MVP Lamination Workflow

The supported workflow is **Draft → Released → Material Allocated → Completed**. Draft schedules may be edited, deleted, released, or cancelled. Release approves and freezes the cutting schedule without reserving or deducting Slit Coil inventory. Only Released jobs may create manual Slit Coil reservations, and confirmation is blocked until every width requirement has zero shortage. Completion is allowed only from Allocated and deducts actual consumed weight, releases unused reservation, and retains the original Slit Coil number. Completed and Cancelled jobs are final and read-only. Start Cutting and In Progress are not available for new Lamination Job workflow actions. The completed Lamination Job is the final production-lot record; no Lamination Batch or individual plate inventory records are created.