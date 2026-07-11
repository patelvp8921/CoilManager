export interface OperationsDashboard {
  dashboardRole: string;
  generatedAt: string;
  kpis: readonly DashboardKpi[];
  inventory: InventorySummary;
  production: ProductionSummary;
  slitting: SlittingSummary;
  slittingJobMetrics: SlittingJobMetrics;
  productionQueue: readonly ProductionQueueItem[];
  quality: QualitySummary;
  procurement: ProcurementSummary;
  dispatch: DispatchSummary;
  analytics: AnalyticsSummary;
  quickActions: readonly QuickAction[];
  recentActivities: readonly RecentActivity[];
  notifications: readonly DashboardNotification[];
}

export interface DashboardKpi {
  label: string;
  value: string;
  icon: string;
  tone: string;
  hint?: string | null;
}

export interface InventorySummary {
  totalMotherCoils: number;
  availableMotherCoils: number;
  reservedMotherCoils: number;
  holdMotherCoils: number;
  rejectedMotherCoils: number;
  totalWeight: number;
  availableWeight: number;
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
