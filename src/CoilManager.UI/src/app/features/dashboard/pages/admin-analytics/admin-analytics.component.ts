import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AnalyticsPlaceholderComponent } from '../../components/analytics-placeholder/analytics-placeholder.component';
import { NotificationPanelComponent } from '../../components/notification-panel/notification-panel.component';
import { QuickActionsComponent } from '../../components/quick-actions/quick-actions.component';
import { RecentActivitiesComponent } from '../../components/recent-activities/recent-activities.component';
import { OperationsDashboard } from '../../models/operations-dashboard.model';
import { DashboardService } from '../../services/dashboard.service';

@Component({
  selector: 'app-admin-analytics',
  imports: [
    DatePipe,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    AnalyticsPlaceholderComponent,
    NotificationPanelComponent,
    QuickActionsComponent,
    RecentActivitiesComponent,
  ],
  templateUrl: './admin-analytics.component.html',
  styleUrl: './admin-analytics.component.scss',
})
export class AdminAnalyticsComponent implements OnInit {
  protected readonly dashboard = signal<OperationsDashboard | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly error = signal('');

  private readonly dashboardService = inject(DashboardService);
  private readonly snackBar = inject(MatSnackBar);

  ngOnInit(): void {
    this.loadAnalytics();
  }

  protected loadAnalytics(): void {
    this.isLoading.set(true);
    this.error.set('');

    this.dashboardService
      .getOperationsDashboard()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (dashboard) => this.dashboard.set(dashboard),
        error: (error: HttpErrorResponse) => {
          const message = this.extractError(error);
          this.error.set(message);
          this.snackBar.open(message, 'Close', { duration: 6000 });
        },
      });
  }

  private extractError(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'The API is not reachable at http://localhost:5170. Start CoilManager.API and try again.';
    }

    const body = error.error as { message?: string; errors?: string[] } | null;
    return body?.errors?.join('\n') || body?.message || error.message || 'Analytics could not be loaded.';
  }
}
