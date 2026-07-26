import { ChangeDetectionStrategy, Component, computed, inject, input, output } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';
import { DashboardKpi, DispatchSummary, SlittingJobMetrics, WorkOrderMetrics } from '../../models/operations-dashboard.model';

export type ProductionStatusTone = 'draft' | 'released' | 'allocated' | 'in-progress' | 'completed' | 'pending' | 'ready';
export interface ProductionStatusMetric { label: string; value: number; tone: ProductionStatusTone; route?: string; status?: number; }
export interface ProductionAreaSummary { id: string; title: string; icon: string; statuses: readonly ProductionStatusMetric[]; }
export interface ProductionStatusSelection { area: ProductionAreaSummary; status: ProductionStatusMetric; }
const PLACEHOLDER_SECTIONS: readonly ProductionAreaSummary[] = [
  { id: 'work-orders', title: 'Work Orders', icon: 'assignment', statuses: [
    { label: 'Released', value: 12, tone: 'released' },
    { label: 'In Progress', value: 8, tone: 'in-progress' },
    { label: 'Completed Today', value: 5, tone: 'completed' },
  ] },
  { id: 'slitting-jobs', title: 'Slitting Jobs', icon: 'content_cut', statuses: [
    { label: 'Draft', value: 3, tone: 'draft' },
    { label: 'Released', value: 5, tone: 'released' },
    { label: 'Completed Today', value: 7, tone: 'completed' },
  ] },
  { id: 'lamination-jobs', title: 'Lamination Jobs', icon: 'precision_manufacturing', statuses: [
    { label: 'Draft', value: 2, tone: 'draft' },
    { label: 'Released', value: 4, tone: 'released' },
    { label: 'Allocated', value: 3, tone: 'allocated' },
    { label: 'Completed Today', value: 6, tone: 'completed' },
  ] },
  { id: 'dispatch', title: 'Dispatch', icon: 'local_shipping', statuses: [
    { label: 'Ready', value: 5, tone: 'ready' },
    { label: 'Dispatched Today', value: 3, tone: 'completed' },
    { label: 'Pending', value: 2, tone: 'pending' },
  ] },
];

@Component({
  selector: 'app-production-summary',
  imports: [MatCardModule, MatIconModule],
  templateUrl: './production-summary.component.html',
  styleUrls: ['../summary-card.scss', './production-summary.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductionSummaryComponent {
  readonly workOrders = input<WorkOrderMetrics | null>(null);
  readonly slittingJobs = input<SlittingJobMetrics | null>(null);
  readonly kpis = input<readonly DashboardKpi[] | null>(null);
  readonly dispatch = input<DispatchSummary | null>(null);
  readonly sections = input<readonly ProductionAreaSummary[] | null>(null);

  readonly statusSelected = output<ProductionStatusSelection>();
  private readonly router = inject(Router);

  protected readonly displaySections = computed<readonly ProductionAreaSummary[]>(() => {
    if (this.sections()) return this.sections()!;
    const workOrders = this.workOrders();
    const slittingJobs = this.slittingJobs();
    const dispatch = this.dispatch();
    if (!workOrders || !slittingJobs || !dispatch || !this.kpis()) return PLACEHOLDER_SECTIONS;
    return [
    {
      id: 'work-orders', title: 'Work Orders', icon: 'assignment',
      statuses: [
        { label: 'Released', value: workOrders.released, tone: 'released', route: '/work-orders', status: 1 },
        { label: 'In Progress', value: workOrders.inProduction, tone: 'in-progress', route: '/work-orders', status: 2 },
        { label: 'Completed Today', value: workOrders.completedToday, tone: 'completed', route: '/work-orders', status: 3 },
        { label: 'Completed', value: workOrders.completed, tone: 'completed', route: '/work-orders', status: 3 },
      ],
    },
    {
      id: 'slitting-jobs', title: 'Slitting Jobs', icon: 'content_cut',
      statuses: [
        { label: 'Draft', value: slittingJobs.draftJobs, tone: 'draft', route: '/slitting-jobs', status: 0 },
        { label: 'Released', value: slittingJobs.releasedJobs, tone: 'released', route: '/slitting-jobs', status: 1 },
        { label: 'In Progress', value: slittingJobs.inProgressJobs, tone: 'in-progress', route: '/slitting-jobs', status: 2 },
        { label: 'Completed Today', value: slittingJobs.completedToday, tone: 'completed', route: '/slitting-jobs', status: 3 },
        { label: 'Completed', value: this.kpiCount('Slitting Jobs', 'Completed'), tone: 'completed', route: '/slitting-jobs', status: 3 },
      ],
    },
    {
      id: 'lamination-jobs', title: 'Lamination Jobs', icon: 'precision_manufacturing',
      statuses: [
        this.kpiMetric('Draft', 'draft'),
        this.kpiMetric('Released', 'released'),
        this.kpiMetric('Allocated', 'allocated'),
        this.kpiMetric('In Progress', 'in-progress'),
        this.kpiMetric('Completed', 'completed'),
      ],
    },
    {
      id: 'dispatch', title: 'Dispatch', icon: 'local_shipping',
      statuses: [
        { label: 'Dispatched', value: dispatch.dispatches, tone: 'completed' },
        { label: 'Pending', value: dispatch.pendingDispatches, tone: 'pending' },
        { label: 'Dispatched Weight (kg)', value: dispatch.dispatchWeight, tone: 'ready' },
      ],
    },
    ];
  });

  protected selectStatus(area: ProductionAreaSummary, status: ProductionStatusMetric): void {
    this.statusSelected.emit({ area, status });
    if (status.route && status.status !== undefined) {
      void this.router.navigate([status.route], { queryParams: { status: status.status } });
    }
  }

  private kpiCount(kpiLabel: string, detailLabel: string): number {
    const count = this.kpis()?.find(kpi => kpi.label === kpiLabel)?.details?.find(detail => detail.label === detailLabel)?.count;
    const value = Number(count?.replace(/,/g, '') ?? 0);
    return Number.isFinite(value) ? value : 0;
  }
  private kpiMetric(label: string, tone: ProductionStatusTone): ProductionStatusMetric {
    const ctlKpi = this.kpis()?.find(kpi => kpi.label === 'CTL Jobs' || kpi.label === 'Lamination Jobs');
    const value = Number(ctlKpi?.details?.find(detail => detail.label === label)?.count.replace(/,/g, '') ?? 0);
    const statusByLabel: Record<string, number> = { Draft: 0, Released: 2, Allocated: 1, 'In Progress': 3, Completed: 4 };
    return { label, value: Number.isFinite(value) ? value : 0, tone, route: '/lamination-jobs', status: statusByLabel[label] };
  }
}