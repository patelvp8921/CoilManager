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
  route?: string | null;
  queryParams?: Readonly<Record<string, string | number>>;
}

@Component({
  selector: 'app-operational-alerts',
  imports: [MatCardModule, MatIconModule, MatRippleModule, MatDialogModule],
  templateUrl: './operational-alerts.component.html',
  styleUrl: './operational-alerts.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OperationalAlertsComponent {
  readonly alerts = input<readonly OperationalAlert[]>([]);
  readonly alertSelected = output<OperationalAlert>();
  protected readonly visibleAlerts = computed(() => this.alerts());
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
