import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { SlittingJobTimelineComponent } from '../../components/slitting-job-timeline/slitting-job-timeline.component';
import { SlittingJob, SlittingJobStatus, slittingJobStatusLabel } from '../../models/slitting-job.model';
import { SlittingJobService } from '../../services/slitting-job.service';

@Component({
  selector: 'app-complete-slitting',
  imports: [
    DatePipe,
    DecimalPipe,
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatChipsModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressBarModule,
    MatSnackBarModule,
    SlittingJobTimelineComponent,
  ],
  templateUrl: './complete-slitting.component.html',
  styleUrl: './complete-slitting.component.scss',
})
export class CompleteSlittingComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  private readonly slittingJobService = inject(SlittingJobService);

  protected readonly job = signal<SlittingJob | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly isSubmitting = signal(false);
  protected readonly errors = signal<readonly string[]>([]);
  protected readonly generatedCoils = signal<readonly string[]>([]);

  protected readonly form = this.fb.nonNullable.group({
    slits: this.fb.array([]),
  });

  protected readonly summary = computed(() => {
    const job = this.job();
    const values = this.slits.controls.map((control) => control.getRawValue());
    const totalActualWeight = values.reduce((sum, row) => sum + this.parseNumber(row.actualWeight), 0);
    const totalPlannedWidth = job?.totalPlannedWidth ?? 0;
    const plannedSlitWidth = job?.items.reduce((sum, item) => sum + item.width, 0) ?? 0;
    const actualSlitWidth = plannedSlitWidth;
    const remainingWidth = job ? Math.max(job.motherCoilWidth - job.knifeLoss - job.edgeTrim - actualSlitWidth, 0) : 0;

    return {
      motherWeight: job?.motherCoilWeight ?? 0,
      totalActualWeight,
      weightDifference: (job?.motherCoilWeight ?? 0) - totalActualWeight,
      totalPlannedWidth,
      plannedSlitWidth,
      actualSlitWidth,
      remainingWidth,
      generatedSlitCount: values.length,
    };
  });

  get slits(): FormArray {
    return this.form.controls.slits;
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.errors.set(['Slitting job id is required.']);
      return;
    }

    this.isLoading.set(true);
    this.slittingJobService.getSlittingJobById(id)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (job) => this.loadJob(job),
        error: (error: HttpErrorResponse) => this.captureError(error),
      });
  }

  protected labelForStatus(status: SlittingJobStatus): string {
    return slittingJobStatusLabel(status);
  }

  protected complete(): void {
    const job = this.job();
    if (!job) {
      return;
    }

    this.errors.set([]);
    this.form.markAllAsTouched();
    const missingRows = this.slits.controls
      .map((control, index) => ({ index: index + 1, weight: this.parseNumber(control.get('actualWeight')?.value) }))
      .filter((row) => row.weight <= 0)
      .map((row) => row.index);

    if (missingRows.length) {
      this.errors.set([`Actual weight is required for row ${missingRows.join(', ')}.`]);
      return;
    }

    this.isSubmitting.set(true);
    this.slittingJobService.completeSlittingJob(job.id, {
      rowVersion: job.rowVersion,
      slits: this.slits.controls.map((control, index) => {
        const value = control.getRawValue();
        return {
          slittingJobItemId: job.items[index].id,
          actualWeight: this.parseNumber(value.actualWeight),
          actualWidth: job.items[index].width,
          remarks: value.remarks || null,
        };
      }),
    })
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (response) => {
          this.generatedCoils.set(response.generatedSlitCoils.map((coil) => coil.coilNumber));
          this.snackBar.open('Slitting completed and slit coil inventory generated successfully.', 'Close', { duration: 4000 });
        },
        error: (error: HttpErrorResponse) => this.captureError(error),
      });
  }

  protected cancelJob(): void {
    const job = this.job();
    if (!job || !window.confirm(`Cancel released slitting job ${job.slittingJobNo}?`)) {
      return;
    }

    this.isSubmitting.set(true);
    this.slittingJobService.cancelSlittingJob(job.id)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: () => {
          this.snackBar.open('Slitting job cancelled and Mother Coil released.', 'Close', { duration: 4000 });
          this.router.navigate(['/slitting-jobs']);
        },
        error: (error: HttpErrorResponse) => this.captureError(error),
      });
  }

  private loadJob(job: SlittingJob): void {
    this.job.set(job);
    this.slits.clear();
    for (const item of [...job.items].sort((left, right) => left.sequenceNo - right.sequenceNo)) {
      this.slits.push(this.fb.nonNullable.group({
        actualWeight: [item.estimatedWeight, [Validators.required, Validators.min(0.001)]],
        remarks: [item.remarks ?? ''],
      }));
    }

    if (job.status !== SlittingJobStatus.InProgress) {
      this.errors.set(['Only in progress slitting jobs can be completed.']);
    }
  }

  private parseNumber(value: unknown): number {
    if (typeof value === 'number') {
      return value;
    }

    return Number(String(value ?? '').trim().replace(/,/g, '')) || 0;
  }

  private captureError(error: HttpErrorResponse): void {
    const body = error.error as { message?: string; errors?: string[] } | null;
    this.errors.set(body?.errors?.length ? body.errors : [body?.message || error.message || 'Request failed.']);
  }
}
