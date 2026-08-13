export enum WorkOrderType {
  CustomerOrder,
  InventoryProduction,
  Rework,
  Trial,
}
export enum WorkOrderProductType {
  MotherCoil,
  SlitCoil,
  Lamination,
  CoreFrameAssembly,
}
export enum WorkOrderStatus {
  Draft,
  Released,
  InFulfilment,
  PartiallyReady,
  Ready,
  Completed,
  OnHold,
  Cancelled,
  PartiallyDispatched,
}
export enum FulfilmentStrategy {
  ExistingInventoryOnly,
  ProductionOnly,
  InventoryAndProduction,
}
export enum ProductionRoute {
  None,
  SlittingOnly,
  LaminationOnly,
  SlittingAndLamination,
}
export enum CoilType {
  MotherCoil = 1,
  SlitCoil = 2,
}
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  errors: readonly string[];
}
export interface ApiPagedResponse<T> extends ApiResponse<readonly T[]> {
  pagination: {
    pageNumber: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
  };
}
export interface WorkOrderListItem {
  id: string;
  workOrderNumber: string;
  workOrderType: WorkOrderType;
  productType: WorkOrderProductType;
  customerName?: string;
  salesOrderReference?: string;
  requiredDate?: string;
  priority: number;
  status: WorkOrderStatus;
  progress: number;
  createdOn: string;
  requiredQuantity?: number;
  quantityUnit: number;
  reservedQuantity: number;
  remainingAllocationQuantity: number;
  allocationPercentage: number;
  linkedLaminationJobId?: string;
  linkedLaminationJobNumber?: string;
  linkedLaminationJobStatus?: number;
  nextActionType: string;
  nextActionLabel: string;
  nextActionRoute?: string;
  hasInventoryShortage: boolean;
  shortageQuantity?: number;
}
export interface WorkOrderOperation {
  id: string;
  operationType: number;
  sequence: number;
  isRequired: boolean;
  status: number;
  relatedDocumentId?: string;
  relatedDocumentNumber?: string;
  startedOn?: string;
  completedOn?: string;
  remarks?: string;
}
export interface Allocation {
  id: string;
  coilType: CoilType;
  coilId: string;
  coilNumber: string;
  allocatedWeight: number;
  issuedWeight?: number;
  consumedWeight?: number;
  remainingWeightAfterAllocation: number;
  status: number;
  reservedOn: string;
  reservedBy: string;
  releasedOn?: string;
}
export interface WorkOrder extends WorkOrderListItem {
  workOrderDate: string;
  gradeId?: string;
  grade?: string;
  thickness: number;
  category: string;
  coreLossPerKg: number;
  drawingNumber?: string;
  requiredWidth?: number;
  requiredWeight?: number;
  requiredQuantity?: number;
  remarks?: string;
  rowVersion: string;
  allocatedWeight: number;
  issuedWeight: number;
  consumedWeight: number;
  operations: readonly WorkOrderOperation[];
  allocations: readonly Allocation[];
  relatedSlittingJobs: readonly {
    id: string;
    slittingJobNumber: string;
    status: number;
  }[];
  fulfilmentStrategy: FulfilmentStrategy;
  planningRequiredQuantity: number;
  plannedInventoryQuantity: number;
  plannedProductionQuantity: number;
  reservedInventoryQuantity: number;
  producedQuantity: number;
  readyQuantity: number;
  dispatchedQuantity: number;
  unplannedQuantity: number;
  coveragePercentage: number;
  productionRoute: ProductionRoute;
  linkedLaminationJob?: WorkOrderLaminationJob;
}
export interface WorkOrderLaminationJob {
  id: string;
  laminationJobNumber: string;
  drawingNumber?: string;
  requiredQuantity: number;
  status: number;
  materialAllocationPercentage: number;
  createdOn: string;
  releasedOn?: string;
  completedOn?: string;
}
export interface AvailableCoil {
  id: string;
  coilType: CoilType;
  coilNumber: string;
  motherCoilNumber?: string;
  grade?: string;
  thickness: number;
  width: number;
  currentWeight: number;
  reservedWeight: number;
  availableWeight: number;
  status: number;
}
export interface WorkOrderRequest {
  workOrderType: WorkOrderType;
  productType: WorkOrderProductType;
  customerName?: string | null;
  salesOrderReference?: string | null;
  salesOrderId?: string | null;
  salesOrderLineId?: string | null;
  workOrderDate: string;
  requiredDate?: string | null;
  priority: number;
  gradeId?: string | null;
  thickness: number;
  category: string;
  coreLossPerKg: number;
  drawingNumber?: string | null;
  requiredWidth?: number | null;
  requiredWeight?: number | null;
  requiredQuantity?: number | null;
  remarks?: string | null;
  fulfilmentStrategy: FulfilmentStrategy;
  plannedInventoryQuantity: number;
  plannedProductionQuantity: number;
  productionRoute: ProductionRoute;
  rowVersion?: string;
}
export const typeLabels = [
  'Customer Order',
  'Inventory Production',
  'Rework',
  'Trial',
];
export const productLabels = [
  'Mother Coil',
  'Slit Coil',
  'Lamination',
  'Core Frame Assembly',
];
export const statusLabels = [
  'Draft',
  'Released',
  'In Fulfilment',
  'Partially Ready',
  'Ready',
  'Completed',
  'On Hold',
  'Cancelled',
  'Partially Dispatched',
];
export const fulfilmentLabels = [
  'Existing Inventory Only',
  'Production Only',
  'Inventory + Production',
];
export const productionRouteLabels = [
  'None',
  'Slitting Only',
  'Lamination Only',
  'Slitting + Lamination',
];
export const operationLabels = ['Slitting', 'Lamination', 'Dispatch'];
export const operationStatusLabels = [
  'Not Required',
  'Pending',
  'In Progress',
  'Completed',
  'Cancelled',
];

export interface WorkOrderNextAction {
  key: string;
  type: string;
  title: string;
  description: string;
  plannedQuantity?: number;
  completedQuantity?: number;
  remainingQuantity?: number;
  quantityUnit?: number;
  status: string;
  severity: string;
  isEnabled: boolean;
  disabledReason?: string;
  actionLabel?: string;
  route?: string;
  actionCode?: string;
  order: number;
}
