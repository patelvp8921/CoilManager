import { DecimalPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { SlittingJob, StartSlittingRequest } from '../../models/slitting-job.model';

@Component({
  selector: 'app-start-slitting-dialog',
  imports: [
    DecimalPipe,
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>play_circle</mat-icon>
      Start Slitting
    </h2>
    <mat-dialog-content>
      <section class="details">
        <div><span>Slitting Job Number</span><strong>{{ job.slittingJobNo }}</strong></div>
        <div><span>Mother Coil Number</span><strong>{{ job.motherCoilNo }}</strong></div>
        <div><span>Planned Slit Count</span><strong>{{ job.items.length }}</strong></div>
        <div><span>Planned Width</span><strong>{{ job.totalPlannedWidth | number:'1.0-3' }} mm</strong></div>
      </section>

      <form [formGroup]="form" class="form-grid">
        <mat-form-field appearance="outline">
          <mat-label>Machine</mat-label>
          <input matInput formControlName="machineId">
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Shift</mat-label>
          <input matInput formControlName="shift">
        </mat-form-field>
        <mat-form-field appearance="outline" class="wide">
          <mat-label>Remarks</mat-label>
          <textarea matInput formControlName="remarks" rows="3"></textarea>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button type="button" (click)="dialogRef.close(null)">Cancel</button>
      <button mat-flat-button type="button" (click)="start()">Start Slitting</button>
    </mat-dialog-actions>
  `,
  styles: [`
    h2 {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .details {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 10px;
      margin-bottom: 16px;
    }

    .details div {
      border: 1px solid #e4e7ec;
      border-radius: 8px;
      padding: 10px;
      background: #f8fafc;
    }

    span {
      display: block;
      color: #667085;
      font-size: 12px;
      line-height: 18px;
    }

    strong {
      display: block;
      color: #101828;
      font-size: 14px;
      line-height: 20px;
    }

    .form-grid {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 12px;
    }

    .wide {
      grid-column: 1 / -1;
    }

    @media (max-width: 620px) {
      .details,
      .form-grid {
        grid-template-columns: 1fr;
      }
    }
  `],
})
export class StartSlittingDialogComponent {
  protected readonly dialogRef = inject(MatDialogRef<StartSlittingDialogComponent, StartSlittingRequest | null>);
  protected readonly job = inject<SlittingJob>(MAT_DIALOG_DATA);
  private readonly fb = inject(FormBuilder);

  protected readonly form = this.fb.nonNullable.group({
    machineId: [''],
    shift: [this.job.shift ?? ''],
    remarks: [this.job.remarks ?? ''],
  });

  protected start(): void {
    const value = this.form.getRawValue();
    this.dialogRef.close({
      rowVersion: this.job.rowVersion,
      machineId: this.toGuidOrNull(value.machineId),
      shift: value.shift || null,
      remarks: value.remarks || null,
    });
  }

  private toGuidOrNull(value: string): string | null {
    const trimmed = value.trim();
    return /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(trimmed)
      ? trimmed
      : null;
  }
}
