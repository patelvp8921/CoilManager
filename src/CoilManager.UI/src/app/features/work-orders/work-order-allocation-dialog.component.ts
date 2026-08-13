import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { Allocation, WorkOrder } from './work-order.model';

export interface WorkOrderAllocationDialogData {
  workOrder: WorkOrder;
  allocations: readonly Allocation[];
}

@Component({
  selector: 'app-work-order-allocation-dialog',
  imports: [DatePipe, DecimalPipe, MatButtonModule, MatDialogModule, MatTableModule],
  template: `
    <h2 mat-dialog-title>Allocated Material</h2>
    <mat-dialog-content>
      <p class="context">{{ data.workOrder.workOrderNumber }} · {{ data.allocations.length }} active allocation(s)</p>
      <div class="table-wrap">
        <table mat-table [dataSource]="data.allocations">
          <ng-container matColumnDef="number"><th mat-header-cell *matHeaderCellDef>Inventory Number</th><td mat-cell *matCellDef="let row"><strong>{{ row.coilNumber }}</strong></td></ng-container>
          <ng-container matColumnDef="type"><th mat-header-cell *matHeaderCellDef>Type</th><td mat-cell *matCellDef="let row">{{ row.coilType === 1 ? 'Mother Coil' : 'Slit Coil' }}</td></ng-container>
          <ng-container matColumnDef="quantity"><th mat-header-cell *matHeaderCellDef>Allocated</th><td mat-cell *matCellDef="let row">{{ row.allocatedWeight | number:'1.0-3' }} {{ data.workOrder.requiredWeight != null ? 'kg' : 'units' }}</td></ng-container>
          <ng-container matColumnDef="reserved"><th mat-header-cell *matHeaderCellDef>Reserved On</th><td mat-cell *matCellDef="let row">{{ row.reservedOn | date:'dd MMM yyyy, h:mm a' }}</td></ng-container>
          <ng-container matColumnDef="by"><th mat-header-cell *matHeaderCellDef>Reserved By</th><td mat-cell *matCellDef="let row">{{ row.reservedBy || '—' }}</td></ng-container>
          <tr mat-header-row *matHeaderRowDef="columns"></tr><tr mat-row *matRowDef="let row; columns: columns"></tr>
        </table>
        @if (!data.allocations.length) { <p class="empty">No active material allocations.</p> }
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end"><button mat-flat-button mat-dialog-close>Close</button></mat-dialog-actions>
  `,
  styles: [`.context{margin-top:0;color:#667085}.table-wrap{max-height:420px;overflow:auto}table{width:100%;min-width:680px}.empty{text-align:center;padding:28px;color:#667085}`],
})
export class WorkOrderAllocationDialogComponent {
  protected readonly data = inject<WorkOrderAllocationDialogData>(MAT_DIALOG_DATA);
  protected readonly columns = ['number', 'type', 'quantity', 'reserved', 'by'];
}
