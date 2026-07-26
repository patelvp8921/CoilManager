import { ChangeDetectionStrategy, Component, computed, inject, input, output } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatRippleModule } from '@angular/material/core';
import { Router } from '@angular/router';

export type AlertSeverity = 'critical' | 'warning' | 'information';
export type AlertCategory =
  | 'material-shortage' | 'inventory-shortage' | 'reservation-conflict' | 'awaiting-allocation'
  | 'production-delayed' | 'dispatch-pending' | 'dispatch-overdue' | 'work-order-overdue'
  | 'slitting-completed' | 'lamination-completed' | 'mother-coil-received';

export interface OperationalAlert {
  id: string;
  severity: AlertSeverity;
  category: AlertCategory | string;
  title: string;
  description: string;
  relativeTime: string;
  ariaLabel: string;
  route?: string;
  queryParams?: Readonly<Record<string, string | number>>;
}

const PLACEHOLDER_ALERTS: readonly OperationalAlert[] = [
  { id: 'material-shortage-wo-1024', severity: 'critical', category: 'material-shortage', title: 'Material Shortage', description: 'WO-1024 requires 350 kg additional Aluminium Coil', relativeTime: '2 minutes ago', ariaLabel: 'View Material Shortage for Work Order WO-1024', route: '/work-orders', queryParams: { search: 'WO-1024' } },
  { id: 'awaiting-allocation-lam-00128', severity: 'warning', category: 'awaiting-allocation', title: 'Awaiting Allocation', description: 'LAM-00128 has not yet been allocated', relativeTime: '15 minutes ago', ariaLabel: 'View Awaiting Allocation for Lamination Job LAM-00128', route: '/lamination-jobs', queryParams: { status: 2, search: 'LAM-00128' } },
  { id: 'dispatch-pending', severity: 'warning', category: 'dispatch-pending', title: 'Dispatch Pending', description: '3 completed jobs are waiting for dispatch', relativeTime: '1 hour ago', ariaLabel: 'View Dispatch Pending items' },
  { id: 'mother-coil-received-mc-000452', severity: 'information', category: 'mother-coil-received', title: 'Mother Coil Received', description: 'MC-000452 received into inventory', relativeTime: 'Today', ariaLabel: 'View received Mother Coil MC-000452', route: '/mother-coils', queryParams: { search: 'MC-000452' } },
  { id: 'slitting-completed-sj-00321', severity: 'information', category: 'slitting-completed', title: 'Slitting Completed', description: 'SJ-00321 completed successfully', relativeTime: 'Today', ariaLabel: 'View completed Slitting Job SJ-00321', route: '/slitting-jobs', queryParams: { status: 3, search: 'SJ-00321' } },
];

@Component({
  selector: 'app-operational-alerts',
  imports: [MatCardModule, MatIconModule, MatRippleModule],
  templateUrl: './operational-alerts.component.html',
  styleUrl: './operational-alerts.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OperationalAlertsComponent {
  readonly alerts = input<readonly OperationalAlert[]>(PLACEHOLDER_ALERTS);
  readonly alertSelected = output<OperationalAlert>();
  protected readonly visibleAlerts = computed(() => this.alerts().slice(0, 5));
  private readonly router = inject(Router);

  protected openAlert(alert: OperationalAlert): void {
    this.alertSelected.emit(alert);
    if (alert.route) void this.router.navigate([alert.route], { queryParams: alert.queryParams });
  }

  protected severityIcon(severity: AlertSeverity): string {
    return severity === 'critical' ? 'error' : severity === 'warning' ? 'warning_amber' : 'info';
  }
}