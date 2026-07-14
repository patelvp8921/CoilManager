namespace CoilManager.Domain.Enums;

public enum WorkOrderType { CustomerOrder, InventoryProduction, Rework, Trial }
public enum WorkOrderProductType { MotherCoil, SlitCoil, Lamination, CoreFrameAssembly }
public enum WorkOrderStatus { Draft, Released, InProduction, Completed, Closed, Cancelled }
public enum WorkOrderOperationType { Slitting, Lamination, Dispatch }
public enum WorkOrderOperationStatus { NotRequired, Pending, InProgress, Completed, Cancelled }
public enum AllocationStatus { Reserved, Issued, PartiallyConsumed, Consumed, Released }
public enum SlittingJobProductionType { Inventory, WorkOrder }
