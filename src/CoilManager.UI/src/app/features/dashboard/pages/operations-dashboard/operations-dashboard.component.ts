import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { DispatchSummaryComponent } from '../../components/dispatch-summary/dispatch-summary.component';
import { InventorySummaryComponent } from '../../components/inventory-summary/inventory-summary.component';
import { KpiCardGridComponent } from '../../components/kpi-card-grid/kpi-card-grid.component';
import { ProcurementSummaryComponent } from '../../components/procurement-summary/procurement-summary.component';
import { ProductionSummaryComponent } from '../../components/production-summary/production-summary.component';
import { QualitySummaryComponent } from '../../components/quality-summary/quality-summary.component';
import { SlittingSummaryComponent } from '../../components/slitting-summary/slitting-summary.component';
import { OperationsDashboard } from '../../models/operations-dashboard.model';
import { DashboardService } from '../../services/dashboard.service';

@Component({
  selector: 'app-operations-dashboard',
  imports: [
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    RouterLink,
    DatePipe,
    DecimalPipe,
    DispatchSummaryComponent,
    InventorySummaryComponent,
    KpiCardGridComponent,
    ProcurementSummaryComponent,
    ProductionSummaryComponent,
    QualitySummaryComponent,
    SlittingSummaryComponent,
  ],
  templateUrl: './operations-dashboard.component.html',
  styleUrl: './operations-dashboard.component.scss',
})
export class OperationsDashboardComponent implements OnInit {
  protected readonly dashboard = signal<OperationsDashboard | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly error = signal('');
  protected readonly dashboardRole = signal(this.resolveDashboardRole());

  private readonly dashboardService = inject(DashboardService);
  private readonly snackBar = inject(MatSnackBar);

  ngOnInit(): void {
    this.loadDashboard();
  }

  protected loadDashboard(): void {
    this.isLoading.set(true);
    this.error.set('');

    this.dashboardService
      .getOperationsDashboard()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (dashboard) => {
          this.dashboard.set(dashboard);
          this.dashboardRole.set(dashboard.dashboardRole || this.resolveDashboardRole());
        },
        error: (error: HttpErrorResponse) => {
          const message = this.extractError(error);
          this.error.set(message);
          this.snackBar.open(message, 'Close', { duration: 6000 });
        },
      });
  }

  private resolveDashboardRole(): string {
    return 'Operations';
  }

  private extractError(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'The API is not reachable at http://localhost:5170. Start CoilManager.API and try again.';
    }

    const body = error.error as { message?: string; errors?: string[] } | null;
    return body?.errors?.join('\n') || body?.message || error.message || 'Dashboard could not be loaded.';
  }
}
