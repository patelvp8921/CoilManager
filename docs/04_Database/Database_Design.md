# Database Design

Sprint 3 adds `app.WorkOrders`, `app.WorkOrderOperations`, and `app.WorkOrderMaterialAllocations` through migration `AddWorkOrdersAndManualMaterialAllocation`.

`WorkOrders.WorkOrderNumber` is unique. Operations have a unique `(WorkOrderId, OperationType)` route entry. Allocations reference exactly one physical Mother or Slit Coil and retain their reservation/release audit fields.

`app.SlittingJobs` gains nullable `WorkOrderId`, `WorkOrderNumber`, and `WorkOrderOperationId`, plus `ProductionType`. Existing rows default to Inventory and remain valid.

Customer and Sales Order foreign keys are intentionally absent. Optional text references are used for the MVP.

## Sprint S1.1 — Customer and Sales Order Foundation

Migration AddCustomerAndSalesOrderFoundation introduces schema sales with Customers, SalesOrders, and SalesOrderLines.

- CustomerCode and SalesOrderNumber have unique indexes.
- Customer name/active status and Sales Order customer/status/order/delivery/PO fields are indexed.
- (SalesOrderId, LineNumber) is unique and lines cascade only with deletion of a Draft order.
- Customer and Grade relationships use restricted deletion.
- Quantities/dimensions use decimal(18,3), core loss uses decimal(18,4), and prices/amounts use decimal(18,2).
- Customer, order, and line tables use SQL row-version concurrency tokens.
- Denormalized customer and grade snapshots retain the commercial/specification context at order time.
# Lamination planning (B4.2A)

`LaminationJobs` owns `LaminationJobSteps`, which own `LaminationJobPlates`, which own flexible `LaminationPlateDimensions`. `LaminationJobMaterialAllocations` links a job to existing `SlitCoils`. Unique constraints enforce job number, step number per job, Plate Type per step, and dimension code per plate. The migration is `AddLaminationJobPlanningAndAllocation`.
