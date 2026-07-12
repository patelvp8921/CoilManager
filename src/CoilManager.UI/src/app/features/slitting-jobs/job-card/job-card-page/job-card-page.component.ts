import { DOCUMENT, DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { SlittingJob, SlittingJobStatus, slittingJobStatusLabel } from '../../models/slitting-job.model';
import { SlittingJobService } from '../../services/slitting-job.service';

@Component({
  selector: 'app-job-card-page',
  imports: [
    DatePipe,
    DecimalPipe,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressBarModule,
    MatSnackBarModule,
  ],
  templateUrl: './job-card-page.component.html',
  styleUrl: './job-card-page.component.scss',
})
export class JobCardPageComponent implements OnInit, OnDestroy {
  protected readonly job = signal<SlittingJob | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly error = signal('');

  private readonly document = inject(DOCUMENT);
  private readonly route = inject(ActivatedRoute);
  private readonly snackBar = inject(MatSnackBar);
  private readonly slittingJobService = inject(SlittingJobService);

  ngOnInit(): void {
    this.document.body.classList.add('job-card-print-mode');

    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.error.set('Slitting job id is required.');
      return;
    }

    this.isLoading.set(true);
    this.slittingJobService.getSlittingJobById(id)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (job) => {
          if (!this.canPrint(job)) {
            this.error.set('Job Card can be printed only for Released, In Progress, or Completed jobs.');
            return;
          }

          this.job.set(job);
        },
        error: (error: HttpErrorResponse) => this.showError(error),
      });
  }

  ngOnDestroy(): void {
    this.document.body.classList.remove('job-card-print-mode');
  }

  protected labelForStatus(status: SlittingJobStatus): string {
    return slittingJobStatusLabel(status);
  }

  protected print(): void {
    window.print();
  }

  protected totalSlitWidth(job: SlittingJob): number {
    return job.items.reduce((sum, item) => sum + item.width, 0);
  }

  private canPrint(job: SlittingJob): boolean {
    return job.status === SlittingJobStatus.Released
      || job.status === SlittingJobStatus.InProgress
      || job.status === SlittingJobStatus.Completed;
  }

  private showError(error: HttpErrorResponse): void {
    const body = error.error as { message?: string; errors?: string[] } | null;
    const message = body?.errors?.join('\n') || body?.message || error.message || 'Request failed.';
    this.error.set(message);
    this.snackBar.open(message, 'Close', { duration: 6000 });
  }
}
