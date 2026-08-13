import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DispatchDetails, dispatchStatusLabels } from './dispatch.model';
import { DispatchService } from './dispatch.service';
import { environment } from '../../../environments/environment';
@Component({
  selector: 'app-dispatch-detail',
  imports: [
    DatePipe,
    DecimalPipe,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
  ],
  template: `@if (item(); as x) {
    <section class="title">
      <div>
        <h1>{{ x.dispatchNumber }}</h1>
        <p>{{ x.packingSlipNumber }} · {{ labels[x.status] }}</p>
      </div>
      <div>
        <a mat-stroked-button routerLink="/dispatch">Back</a>
        @if (x.status === 0) {
          <button mat-flat-button (click)="confirm()">Confirm Dispatch</button
          ><button mat-button class="danger" (click)="cancel()">Cancel</button>
        }
        <button mat-stroked-button (click)="preview()">
          Preview Packing Slip</button
        ><a mat-stroked-button [href]="pdf(x.id)" target="_blank"
          >Download PDF</a
        >
      </div>
    </section>
    <main>
      <mat-card
        ><h2>Dispatch Summary</h2>
        <div class="facts">
          <div>
            <span>Customer</span><strong>{{ x.customerName }}</strong>
          </div>
          <div>
            <span>Work Order</span><strong>{{ x.workOrderNumber }}</strong>
          </div>
          <div>
            <span>Product</span><strong>{{ x.description }}</strong>
          </div>
          <div>
            <span>Quantity</span
            ><strong
              >{{ x.dispatchQuantity | number: '1.0-3' }}
              {{ unit(x.quantityUnit) }}</strong
            >
          </div>
          <div>
            <span>Packages</span><strong>{{ x.packageCount }}</strong>
          </div>
          <div>
            <span>Vehicle</span><strong>{{ x.vehicleNumber || '—' }}</strong>
          </div>
        </div></mat-card
      ><mat-card
        ><h2>Packages</h2>
        <table>
          <tr>
            <th>Package</th>
            <th>Description</th>
            <th>Quantity</th>
            <th>Net</th>
            <th>Gross</th>
          </tr>
          @for (p of x.packages; track p.packageNumber) {
            <tr>
              <td>{{ p.packageNumber }}</td>
              <td>{{ p.description }}</td>
              <td>{{ p.quantity }}</td>
              <td>{{ p.netWeight }}</td>
              <td>{{ p.grossWeight }}</td>
            </tr>
          }
        </table></mat-card
      >
      @if (x.inventorySources.length) {
        <mat-card
          ><h2>Inventory Traceability</h2>
          <table>
            <tr>
              <th>Coil</th>
              <th>Width</th>
              <th>Dispatched</th>
            </tr>
            @for (s of x.inventorySources; track s.inventoryNumber) {
              <tr>
                <td>{{ s.inventoryNumber }}</td>
                <td>{{ s.width }} mm</td>
                <td>{{ s.quantity | number: '1.0-3' }} kg</td>
              </tr>
            }
          </table></mat-card
        >
      }
    </main>
  }`,
  styles: [
    `
      :host {
        display: block;
        background: #f5f7fa;
        min-height: 100%;
      }
      .title {
        display: flex;
        justify-content: space-between;
        padding: 24px 32px;
        background: #fff;
      }
      .title h1 {
        margin: 0;
      }
      .title p {
        color: #667085;
      }
      .title > div:last-child {
        display: flex;
        gap: 8px;
        align-items: center;
      }
      .danger {
        color: #b42318;
      }
      main {
        display: grid;
        gap: 14px;
        padding: 20px;
      }
      mat-card {
        padding: 18px;
      }
      .facts {
        display: grid;
        grid-template-columns: repeat(3, 1fr);
        gap: 12px;
      }
      .facts div {
        border: 1px solid #ddd;
        padding: 12px;
      }
      .facts span {
        display: block;
        color: #667085;
        font-size: 11px;
      }
      table {
        width: 100%;
        border-collapse: collapse;
      }
      td,
      th {
        padding: 10px;
        border-bottom: 1px solid #ddd;
        text-align: left;
      }
    `,
  ],
})
export class DispatchDetailComponent implements OnInit {
  item = signal<DispatchDetails | null>(null);
  labels = dispatchStatusLabels;
  private api = inject(DispatchService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private snack = inject(MatSnackBar);
  private id = '';
  ngOnInit() {
    this.id = this.route.snapshot.paramMap.get('id')!;
    if (this.id === 'create') {
      void this.router.navigate(['/dispatch-create'], {
        queryParams: {
          workOrderId: this.route.snapshot.queryParamMap.get('workOrderId'),
        },
      });
      return;
    }
    this.load();
  }
  load() {
    this.api.get(this.id).subscribe((x) => this.item.set(x));
  }
  confirm() {
    if (
      !confirm(
        'Confirm dispatch? Inventory will be deducted and this cannot be casually reversed.',
      )
    )
      return;
    this.api.confirm(this.id).subscribe({
      next: () => {
        this.snack.open('Dispatch confirmed.', 'Close', { duration: 3000 });
        this.load();
      },
      error: (e) =>
        this.snack.open(e?.error?.message || e.message, 'Close', {
          duration: 6000,
        }),
    });
  }
  cancel() {
    const reason = prompt('Cancellation reason');
    if (!reason) return;
    this.api
      .cancel(this.id, reason, this.item()!.rowVersion)
      .subscribe(() => this.load());
  }
  preview() {
    window.open(this.api.packing(this.id), '_blank');
  }
  pdf(id: string) {
    return this.api.pdf(id);
  }
  unit(x: number) {
    return ['kg', 'pieces', 'sets'][x];
  }
}
