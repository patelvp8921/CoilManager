export interface OperationsDashboard {
  dashboardRole: string;
  generatedAt: string;
  kpis: readonly DashboardKpi[];
  inventory: InventorySummary;
  production: ProductionSummary;
  workOrders: WorkOrderMetrics;
  slitting: SlittingSummary;
  slittingJobMetrics: SlittingJobMetrics;
  productionQueue: readonly ProductionQueueItem[];
  laminationProductionQueue: readonly LaminationProductionQueueItem[];
  operationalAlerts: readonly OperationalAlert[];
  quality: QualitySummary;
  procurement: ProcurementSummary;
  dispatch: DispatchSummary;
  analytics: AnalyticsSummary;
  quickActions: readonly QuickAction[];
  recentActivities: readonly RecentActivity[];
  notifications: readonly DashboardNotification[];
}

export interface WorkOrderMetrics { draft: number; released: number; inProduction: number; completedToday: number; completed: number; overdue: number; customerWorkOrders: number; inventoryProductionWorkOrders: number; queue: readonly WorkOrderQueueItem[]; }
export interface WorkOrderQueueItem { id: string; workOrderNumber: string; productType: number; requiredDate?: string | null; priority: number; status: number; allocationPercentage: number; operationProgress: number; }

export interface DashboardKpi {
  label: string;
  value: string;
  icon: string;
  tone: string;
  hint?: string | null;
  details?: readonly DashboardKpiDetail[] | null;
}

export interface DashboardKpiDetail {
  label: string;
  count: string;
  weight?: string | null;
}

export interface InventorySummary {
  totalMotherCoils: number;
  availableMotherCoils: number;
  consumedMotherCoils?: number;
  reservedMotherCoils: number;
  holdMotherCoils: number;
  rejectedMotherCoils: number;
  totalWeight: number;
  availableWeight: number;
  totalSlitCoils: number;
  availableSlitCoils: number;
  consumedSlitCoils: number;
  inProcessSlitCoils: number;
  totalSlitWeight: number;
  availableSlitWeight: number;
  slitGradeWiseStock: readonly InventoryBreakdown[];
  gradeWiseStock: readonly InventoryBreakdown[];
  supplierWiseStock: readonly InventoryBreakdown[];
  recentReceivedCoils: readonly RecentMotherCoil[];
}

export interface InventoryBreakdown {
  name: string;
  count: number;
  weight: number;
}

export interface RecentMotherCoil {
  id: string;
  coilId: string;
  grade: string;
  supplier: string;
  weight: number;
  receivedDate: string;
  status: string;
}

export interface ProductionSummary {
  workOrders: number;
  finishedCoils: number;
  plannedWeight: number;
  producedWeight: number;
  status: string;
}

export interface SlittingSummary {
  slitCoils: number;
  slittingJobs: number;
  slitWeight: number;
  status: string;
}

export interface SlittingJobMetrics {
  draftJobs: number;
  releasedJobs: number;
  inProgressJobs: number;
  completedToday: number;
  cancelledJobs: number;
  averageWaitingMinutes: number;
  averageProcessingMinutes: number;
}

export interface ProductionQueueItem {
  slittingJobId: string;
  slittingJobNo: string;
  motherCoilNumber: string;
  status: string;
  releasedOn?: string | null;
  startedOn?: string | null;
  waitingMinutes: number;
  machineId?: string | null;
  shift?: string | null;
  route: string;
}

export interface LaminationProductionQueueItem {
  id: string;
  jobNumber: string;
  drawingNumber?: string | null;
  rating: string;
  status: number;
  allocationPercentage: number;
  plannedDate: string;
  machine?: string | null;
  shift?: string | null;
}
export interface OperationalAlert {
  id: string;
  severity: 'critical' | 'warning' | 'information';
  category: string;
  title: string;
  description: string;
  relativeTime: string;
  ariaLabel: string;
  route?: string | null;
}
export interface QualitySummary {
  pendingQa: number;
  holdCoils: number;
  rejectedCoils: number;
  status: string;
}

export interface ProcurementSummary {
  pendingReceipts: number;
  suppliers: number;
  incomingWeight: number;
  status: string;
}

export interface DispatchSummary {
  dispatches: number;
  dispatchWeight: number;
  pendingDispatches: number;
  status: string;
}

export interface AnalyticsSummary {
  cards: readonly AnalyticsPlaceholder[];
}

export interface AnalyticsPlaceholder {
  title: string;
  description: string;
  icon: string;
}

export interface QuickAction {
  label: string;
  icon: string;
  route?: string | null;
  enabled: boolean;
  badge?: string | null;
}

export interface RecentActivity {
  title: string;
  description: string;
  timestamp: string;
  icon: string;
  tone: string;
  route?: string | null;
}

export interface DashboardNotification {
  title: string;
  message: string;
  severity: string;
  icon: string;
  route?: string | null;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  errors: readonly string[];
}
