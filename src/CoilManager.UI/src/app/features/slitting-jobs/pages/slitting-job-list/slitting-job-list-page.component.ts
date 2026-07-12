import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSort, MatSortModule, Sort } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import {
  SLITTING_JOB_STATUS_OPTIONS,
  SlittingJob,
  SlittingJobQuery,
  SlittingJobStatus,
  slittingJobStatusLabel,
} from '../../models/slitting-job.model';
import { SlittingJobService } from '../../services/slitting-job.service';
import { StartSlittingDialogComponent } from '../../components/start-slitting-dialog/start-slitting-dialog.component';

@Component({
  selector: 'app-slitting-job-list-page',
  imports: [
    DatePipe,
    DecimalPipe,
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatPaginatorModule,
    MatProgressBarModule,
    MatSelectModule,
    MatSnackBarModule,
    MatSortModule,
    MatTableModule,
  ],
  templateUrl: './slitting-job-list-page.component.html',
  styleUrl: './slitting-job-list-page.component.scss',
})
export class SlittingJobListPageComponent implements OnInit {
  @ViewChild(MatPaginator) private paginator?: MatPaginator;
  @ViewChild(MatSort) private sort?: MatSort;

  protected readonly statusOptions = SLITTING_JOB_STATUS_OPTIONS;
  protected readonly displayedColumns = [
    'slittingJobNo',
    'planningDate',
    'motherCoilNo',
    'grade',
    'thickness',
    'motherCoilWidth',
    'motherCoilWeight',
    'totalPlannedWidth',
    'status',
    'actions',
  ];

  protected readonly searchControl = new FormControl('', { nonNullable: true });
  protected readonly statusControl = new FormControl<SlittingJobStatus | null>(null);

  protected readonly slittingJobs = signal<readonly SlittingJob[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly pageSize = signal(25);
  protected readonly pageIndex = signal(0);
  protected readonly isLoading = signal(false);

  private readonly slittingJobService = inject(SlittingJobService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  ngOnInit(): void {
    this.loadSlittingJobs();
  }

  protected applyFilters(): void {
    this.pageIndex.set(0);
    this.paginator?.firstPage();
    this.loadSlittingJobs();
  }

  protected resetFilters(): void {
    this.searchControl.reset('');
    this.statusControl.reset(null);
    this.pageIndex.set(0);
    this.sort?.sort({ id: '', start: 'asc', disableClear: false });
    this.paginator?.firstPage();
    this.loadSlittingJobs();
  }

  protected onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.loadSlittingJobs();
  }

  protected onSortChange(sort: Sort): void {
    this.pageIndex.set(0);
    this.paginator?.firstPage();
    this.loadSlittingJobs(sort);
  }

  protected labelForStatus(status: SlittingJobStatus): string {
    return slittingJobStatusLabel(status);
  }

  protected canEdit(job: SlittingJob): boolean {
    return job.status === SlittingJobStatus.Draft;
  }

  protected canComplete(job: SlittingJob): boolean {
    return job.status === SlittingJobStatus.InProgress;
  }

  protected canStart(job: SlittingJob): boolean {
    return job.status === SlittingJobStatus.Released;
  }

  protected canPrintJobCard(job: SlittingJob): boolean {
    return job.status === SlittingJobStatus.Released
      || job.status === SlittingJobStatus.InProgress
      || job.status === SlittingJobStatus.Completed;
  }

  protected canCancel(job: SlittingJob): boolean {
    return job.status === SlittingJobStatus.Released;
  }

  protected canViewSlitCoils(job: SlittingJob): boolean {
    return job.status === SlittingJobStatus.Completed;
  }

  protected start(job: SlittingJob): void {
    const ref = this.dialog.open(StartSlittingDialogComponent, {
      width: '620px',
      data: job,
    });

    ref.afterClosed().subscribe((request) => {
      if (!request) {
        return;
      }

      this.isLoading.set(true);
      this.slittingJobService
        .startSlittingJob(job.id, request)
        .pipe(finalize(() => this.isLoading.set(false)))
        .subscribe({
          next: () => {
            this.snackBar.open('Slitting started successfully.', 'Close', { duration: 4000 });
            this.loadSlittingJobs();
          },
          error: (error: HttpErrorResponse) => this.showError(error),
        });
    });
  }

  protected cancel(job: SlittingJob): void {
    if (!window.confirm(`Cancel released slitting job ${job.slittingJobNo}?`)) {
      return;
    }

    this.isLoading.set(true);
    this.slittingJobService
      .cancelSlittingJob(job.id)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: () => {
          this.snackBar.open('Slitting job cancelled and Mother Coil released.', 'Close', { duration: 4000 });
          this.loadSlittingJobs();
        },
        error: (error: HttpErrorResponse) => this.showError(error),
      });
  }

  protected loadSlittingJobs(sort: Sort | null = this.sort ?? null): void {
    const query: SlittingJobQuery = {
      page: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      search: this.searchControl.value.trim(),
      status: this.statusControl.value,
      sortBy: sort?.active,
      sortDirection: sort?.direction,
    };

    this.isLoading.set(true);
    this.slittingJobService
      .getSlittingJobs(query)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          this.slittingJobs.set(response.data);
          this.totalCount.set(response.pagination.totalCount);
        },
        error: (error: HttpErrorResponse) => this.showError(error),
      });
  }

  private showError(error: HttpErrorResponse): void {
    const message = this.extractError(error);
    this.snackBar.open(message, 'Close', { duration: 6000 });
  }

  private extractError(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'The API is not reachable at http://localhost:5170. Start CoilManager.API and try again.';
    }

    const body = error.error as { message?: string; errors?: string[] } | null;
    return body?.errors?.join('\n') || body?.message || error.message || 'Request failed.';
  }
}
