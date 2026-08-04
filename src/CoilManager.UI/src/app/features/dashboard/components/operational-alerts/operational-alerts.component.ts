import { ChangeDetectionStrategy, Component, computed, inject, input, output } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatRippleModule } from '@angular/material/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { OperationalAlertDetailDialogComponent } from './operational-alert-detail-dialog.component';

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
];

const VISIBLE_CATEGORIES = new Set(['material-shortage', 'awaiting-allocation', 'dispatch-pending']);

@Component({
  selector: 'app-operational-alerts',
  imports: [MatCardModule, MatIconModule, MatRippleModule, MatDialogModule],
  templateUrl: './operational-alerts.component.html',
  styleUrl: './operational-alerts.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OperationalAlertsComponent {
  readonly alerts = input<readonly OperationalAlert[]>(PLACEHOLDER_ALERTS);
  readonly alertSelected = output<OperationalAlert>();
  protected readonly visibleAlerts = computed(() => this.alerts().filter(alert => VISIBLE_CATEGORIES.has(alert.category)));
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);

  protected openAlert(alert: OperationalAlert): void {
    this.alertSelected.emit(alert);
    const dialogRef = this.dialog.open(OperationalAlertDetailDialogComponent, {
      data: alert,
      width: 'min(520px, calc(100vw - 32px))',
      maxWidth: '520px',
      autoFocus: 'dialog',
    });
    dialogRef.afterClosed().subscribe(openModule => {
      if (openModule && alert.route) void this.router.navigate([alert.route], { queryParams: alert.queryParams });
    });
  }

  protected severityIcon(severity: AlertSeverity): string {
    return severity === 'critical' ? 'error' : severity === 'warning' ? 'warning_amber' : 'info';
  }
}
