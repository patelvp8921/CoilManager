namespace CoilManager.Domain.Enums;

public enum WorkOrderSourceType { SalesOrder, StockProduction }
public enum FulfilmentStrategy { ExistingInventoryOnly, ProductionOnly, InventoryAndProduction }
public enum ProductionRoute { None, SlittingOnly, LaminationOnly, SlittingAndLamination }
public enum WorkOrderStatus { Draft, Released, InFulfilment, PartiallyReady, Ready, Completed, OnHold, Cancelled, PartiallyDispatched, InProduction = InFulfilment, Closed = Completed }
public enum DispatchStatus { Draft, Dispatched, Cancelled }
public enum WorkOrderType { CustomerOrder, InventoryProduction, Rework, Trial }
public enum WorkOrderProductType { MotherCoil, SlitCoil, Lamination, CoreFrameAssembly }
public enum WorkOrderOperationType { Slitting, Lamination, Dispatch }
public enum WorkOrderOperationStatus { NotRequired, Pending, InProgress, Completed, Cancelled }
public enum AllocationStatus { Reserved, Issued, PartiallyConsumed, Consumed, Released }
public enum SlittingJobProductionType { Inventory, WorkOrder }
