# Database Design

Sprint 3 adds `app.WorkOrders`, `app.WorkOrderOperations`, and `app.WorkOrderMaterialAllocations` through migration `AddWorkOrdersAndManualMaterialAllocation`.

`WorkOrders.WorkOrderNumber` is unique. Operations have a unique `(WorkOrderId, OperationType)` route entry. Allocations reference exactly one physical Mother or Slit Coil and retain their reservation/release audit fields.

`app.SlittingJobs` gains nullable `WorkOrderId`, `WorkOrderNumber`, and `WorkOrderOperationId`, plus `ProductionType`. Existing rows default to Inventory and remain valid.

Customer and Sales Order foreign keys are intentionally absent. Optional text references are used for the MVP.
# Lamination planning (B4.2A)

`LaminationJobs` owns `LaminationJobSteps`, which own `LaminationJobPlates`, which own flexible `LaminationPlateDimensions`. `LaminationJobMaterialAllocations` links a job to existing `SlitCoils`. Unique constraints enforce job number, step number per job, Plate Type per step, and dimension code per plate. The migration is `AddLaminationJobPlanningAndAllocation`.
