import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-delete-confirm-dialog',
  imports: [MatButtonModule, MatDialogModule, MatIconModule],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>delete</mat-icon>
      Delete raw coil
    </h2>
    <mat-dialog-content>
      Delete <strong>{{ data.coilNumber }}</strong>? This marks the coil as deleted.
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button type="button" (click)="dialogRef.close(false)">Cancel</button>
      <button mat-flat-button color="warn" type="button" (click)="dialogRef.close(true)">Delete</button>
    </mat-dialog-actions>
  `,
  styles: [
    `
      h2 {
        display: flex;
        align-items: center;
        gap: 8px;
      }
    `,
  ],
})
export class DeleteConfirmDialogComponent {
  protected readonly dialogRef = inject(MatDialogRef<DeleteConfirmDialogComponent, boolean>);
  protected readonly data = inject<{ coilNumber: string }>(MAT_DIALOG_DATA);
}
