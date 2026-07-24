import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { CoilTraceability } from '../coil-search/models/coil.model';
import { SlitCoilDetails } from '../slit-coils/models/slit-coil.model';

type DialogData = { mode: 'coil'; coil: SlitCoilDetails } | { mode: 'traceability'; traceability: CoilTraceability };

@Component({
  selector: 'app-allocation-coil-detail-dialog',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatCardModule, MatDialogModule, MatIconModule],
  template: `
    @if (data.mode === 'coil') {
      <h2 mat-dialog-title><mat-icon>view_in_ar</mat-icon> Slit Coil {{data.coil.coilNumber}}</h2>
      <mat-dialog-content>
        <section class="detail-grid">
          <div><span>Mother Coil</span><strong>{{data.coil.motherCoilNumber || '—'}}</strong></div>
          <div><span>Parent Coil</span><strong>{{data.coil.parentCoilNumber || '—'}}</strong></div>
          <div><span>Slitting Job</span><strong>{{data.coil.slittingJobNo || '—'}}</strong></div>
          <div><span>Status</span><strong>{{status(data.coil.status)}}</strong></div>
          <div><span>Grade</span><strong>{{data.coil.grade || '—'}}</strong></div>
          <div><span>Thickness</span><strong>{{data.coil.thickness | number:'1.3-3'}} mm</strong></div>
          <div><span>Width</span><strong>{{data.coil.width | number:'1.3-3'}} mm</strong></div>
          <div><span>Weight</span><strong>{{data.coil.weight | number:'1.3-3'}} kg</strong></div>
          <div><span>Core Loss</span><strong>{{data.coil.coreLossPerKg | number:'1.2-4'}} W/kg</strong></div>
          <div><span>Category</span><strong>{{data.coil.category || '—'}}</strong></div>
          <div><span>Warehouse</span><strong>{{data.coil.warehouseLocation || '—'}}</strong></div>
          <div><span>Created On</span><strong>{{data.coil.createdOn | date:'medium'}}</strong></div>
        </section>
      </mat-dialog-content>
    } @else {
      <h2 mat-dialog-title><mat-icon>account_tree</mat-icon> Traceability · {{data.traceability.currentCoil.coilNumber}}</h2>
      <mat-dialog-content>
        <h3>Coil Genealogy</h3>
        <div class="genealogy tree-wrap">
          <ng-template #treeNode let-node>
            <div class="tree-node">
              <article [class.current]="node.id === data.traceability.currentCoil.id">
                <mat-icon>{{node.coilType === 1 ? 'inventory_2' : 'view_stream'}}</mat-icon>
                <div><strong>{{node.coilNumber}}</strong><span>{{node.coilType === 1 ? 'Mother Coil' : 'Slit Coil'}} · {{node.width | number:'1.3-3'}} mm · {{node.weight | number:'1.3-3'}} kg</span></div>
              </article>
              @if (node.children.length) {
                <div class="children">
                  @for (child of node.children; track child.id) {
                    <ng-container [ngTemplateOutlet]="treeNode" [ngTemplateOutletContext]="{$implicit: child}" />
                  }
                </div>
              }
            </div>
          </ng-template>
          <ng-container [ngTemplateOutlet]="treeNode" [ngTemplateOutletContext]="{$implicit: data.traceability.rootMotherCoil}" />
        </div>
        <h3>Current Coil Details</h3>
        <section class="detail-grid">
          <div><span>Grade</span><strong>{{data.traceability.currentCoil.grade || '—'}}</strong></div>
          <div><span>Thickness</span><strong>{{data.traceability.currentCoil.thickness | number:'1.3-3'}} mm</strong></div>
          <div><span>Parent</span><strong>{{data.traceability.currentCoil.parentCoilNumber || '—'}}</strong></div>
          <div><span>Root Mother Coil</span><strong>{{data.traceability.currentCoil.rootMotherCoilNumber}}</strong></div>
          <div><span>Slitting Job</span><strong>{{data.traceability.currentCoil.slittingJobNo || '—'}}</strong></div>
          <div><span>Created On</span><strong>{{data.traceability.currentCoil.createdOn | date:'medium'}}</strong></div>
        </section>
        <h3>Inventory Timeline</h3>
        <div class="timeline">
          @for (item of data.traceability.inventoryTransactions; track item.id) {
            <article><mat-icon>history</mat-icon><div><strong>{{item.relatedDocumentNumber || 'Inventory Movement'}}</strong><span>{{item.remarks || 'Status updated'}} · {{item.quantityWeight | number:'1.3-3'}} kg</span><small>{{item.transactionDate | date:'medium'}}</small></div></article>
          } @empty { <p>No inventory timeline entries are available.</p> }
        </div>
      </mat-dialog-content>
    }
    <mat-dialog-actions align="end"><button mat-flat-button mat-dialog-close>Close</button></mat-dialog-actions>
  `,
  styles: [`
    h2{display:flex;align-items:center;gap:8px}h3{margin:20px 0 10px;font-size:14px}mat-dialog-content{width:min(900px,85vw);max-height:72vh}.detail-grid{display:grid;grid-template-columns:repeat(3,minmax(150px,1fr));gap:10px}.detail-grid div{display:grid;gap:4px;border:1px solid #e4e9f0;border-radius:8px;padding:12px;background:#f8fafc}.detail-grid span,.genealogy span,.timeline span,.timeline small{color:#667085;font-size:11px}.genealogy,.timeline{display:grid;gap:8px}.genealogy article,.timeline article{display:flex;align-items:center;gap:10px;border:1px solid #e4e9f0;border-radius:8px;padding:10px 12px}.genealogy article.current{border-color:#84adff;background:#eef4ff}.genealogy article div,.timeline article div{display:grid;gap:3px}.tree-wrap{overflow:auto;padding:10px}.tree-node{position:relative;display:flex;align-items:flex-start;gap:22px}.tree-node>.children{position:relative;display:grid;gap:10px;padding-left:22px}.tree-node>.children:before{position:absolute;top:20px;bottom:20px;left:0;border-left:2px solid #cbd5e1;content:''}.tree-node>.children>.tree-node:before{position:absolute;top:20px;left:-22px;width:22px;border-top:2px solid #cbd5e1;content:''}.tree-node article{min-width:190px}@media(max-width:700px){.detail-grid{grid-template-columns:1fr 1fr}mat-dialog-content{width:82vw}.tree-node{gap:12px}.tree-node>.children{padding-left:12px}.tree-node>.children>.tree-node:before{left:-12px;width:12px}}
  `]
})
export class AllocationCoilDetailDialogComponent {
  readonly data = inject<DialogData>(MAT_DIALOG_DATA);
  status(value: number): string { return ['Available', 'Reserved', 'Consumed', 'Scrapped', 'Blocked'][value] ?? `${value}`; }
}
