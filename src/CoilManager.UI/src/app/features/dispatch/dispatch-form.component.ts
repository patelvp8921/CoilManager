import { Component, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { forkJoin } from 'rxjs';
import { Customer } from '../sales/sales.model';
import { SalesService } from '../sales/sales.service';
import { DispatchPackage, DispatchSummary } from './dispatch.model';
import { DispatchService } from './dispatch.service';
@Component({
  selector: 'app-dispatch-form',
  imports: [
    DecimalPipe,
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule,
  ],
  template: `<section class="title">
      <div>
        <h1>Create Dispatch</h1>
        <p>
          Prepare shipping information and package details. Saving Draft does
          not consume inventory.
        </p>
      </div>
      <a mat-stroked-button routerLink="/work-orders">Back</a>
    </section>
    @if (summary(); as s) {
      <main>
        <mat-card
          ><h2>Dispatch Header</h2>
          <div class="facts">
            <div>
              <span>Work Order</span><strong>{{ s.workOrderNumber }}</strong>
            </div>
            <div>
              <span>Required</span
              ><strong
                >{{ s.requiredQuantity | number: '1.0-3' }}
                {{ unit(s.quantityUnit) }}</strong
              >
            </div>
            <div>
              <span>Ready</span
              ><strong>{{ s.totalFulfilledQuantity | number: '1.0-3' }}</strong>
            </div>
            <div>
              <span>Previously Dispatched</span
              ><strong>{{
                s.totalDispatchedQuantity | number: '1.0-3'
              }}</strong>
            </div>
            <div>
              <span>Remaining</span
              ><strong>{{
                s.availableForDispatchQuantity | number: '1.0-3'
              }}</strong>
            </div>
          </div></mat-card
        >
        <form [formGroup]="form" (ngSubmit)="save()">
          <mat-card
            ><h2>Customer & Delivery</h2>
            <div class="grid">
              <mat-form-field appearance="outline" class="wide">
                <mat-label>Customer</mat-label>
                <mat-select formControlName="customerId" (selectionChange)="customerChanged($event.value)">
                  @for (customer of customers(); track customer.id) {
                    <mat-option [value]="customer.id">{{ customer.customerCode }} - {{ customer.customerName }}</mat-option>
                  }
                </mat-select>
              </mat-form-field>
              <mat-form-field appearance="outline" class="wide"
                ><mat-label>Shipping Address</mat-label
                ><textarea
                  matInput
                  rows="3"
                  formControlName="shippingAddress"
                ></textarea></mat-form-field
              ><mat-form-field appearance="outline"
                ><mat-label>Contact Person</mat-label
                ><input
                  matInput
                  formControlName="contactPerson" /></mat-form-field
              ><mat-form-field appearance="outline"
                ><mat-label>Contact Phone</mat-label
                ><input matInput formControlName="contactPhone"
              /></mat-form-field></div></mat-card
          ><mat-card
            ><h2>Dispatch & Packing</h2>
            <div class="grid">
              <mat-form-field appearance="outline"
                ><mat-label>Dispatch Quantity</mat-label
                ><input
                  matInput
                  type="number"
                  step="0.001"
                  formControlName="dispatchQuantity" /></mat-form-field
              ><mat-form-field appearance="outline"
                ><mat-label>Dispatch Date</mat-label
                ><input
                  matInput
                  type="date"
                  formControlName="dispatchDate" /></mat-form-field
              ><mat-form-field appearance="outline"
                ><mat-label>Net Weight</mat-label
                ><input
                  matInput
                  type="number"
                  formControlName="netWeight" /></mat-form-field
              ><mat-form-field appearance="outline"
                ><mat-label>Gross Weight</mat-label
                ><input matInput type="number" formControlName="grossWeight"
              /></mat-form-field>
            </div>
            <h3>Packages</h3>
            @for (p of packages(); track $index) {
              <div class="package">
                <input
                  placeholder="Package"
                  [value]="p.packageNumber"
                  (input)="setPackage($index, 'packageNumber', $event)"
                /><input
                  placeholder="Description"
                  [value]="p.description"
                  (input)="setPackage($index, 'description', $event)"
                /><input
                  type="number"
                  placeholder="Quantity"
                  [value]="p.quantity ?? ''"
                  (input)="setPackage($index, 'quantity', $event, true)"
                /><input
                  type="number"
                  placeholder="Net kg"
                  [value]="p.netWeight ?? ''"
                  (input)="setPackage($index, 'netWeight', $event, true)"
                /><input
                  type="number"
                  placeholder="Gross kg"
                  [value]="p.grossWeight ?? ''"
                  (input)="setPackage($index, 'grossWeight', $event, true)"
                /><button
                  mat-icon-button
                  type="button"
                  (click)="remove($index)"
                >
                  <mat-icon>delete</mat-icon>
                </button>
              </div>
            }
            <button mat-stroked-button type="button" (click)="add()">
              <mat-icon>add</mat-icon>Add Package
            </button></mat-card
          ><mat-card
            ><h2>Transport Details</h2>
            <div class="grid">
              <mat-form-field appearance="outline"
                ><mat-label>Transporter</mat-label
                ><input
                  matInput
                  formControlName="transporterName" /></mat-form-field
              ><mat-form-field appearance="outline"
                ><mat-label>Vehicle Number</mat-label
                ><input
                  matInput
                  formControlName="vehicleNumber" /></mat-form-field
              ><mat-form-field appearance="outline"
                ><mat-label>LR/GR Number</mat-label
                ><input matInput formControlName="lrgrNumber" /></mat-form-field
              ><mat-form-field appearance="outline"
                ><mat-label>E-Way Bill</mat-label
                ><input
                  matInput
                  formControlName="eWayBillNumber" /></mat-form-field
              ><mat-form-field appearance="outline" class="wide"
                ><mat-label>Packing Remarks</mat-label
                ><textarea
                  matInput
                  formControlName="packingRemarks"
                ></textarea></mat-form-field
              ><mat-form-field appearance="outline" class="wide"
                ><mat-label>Dispatch Remarks</mat-label
                ><textarea
                  matInput
                  formControlName="dispatchRemarks"
                ></textarea>
              </mat-form-field></div
          ></mat-card>
          <footer>
            <a mat-stroked-button routerLink="/work-orders">Cancel</a
            ><button mat-flat-button type="submit" [disabled]="form.invalid">
              Save Draft & Generate Packing Slip
            </button>
          </footer>
        </form>
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
      main,
      form {
        display: grid;
        gap: 14px;
        padding: 16px 24px;
      }
      form {
        padding: 0;
      }
      mat-card {
        padding: 18px;
      }
      .facts {
        display: grid;
        grid-template-columns: repeat(5, 1fr);
        gap: 10px;
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
      .grid {
        display: grid;
        grid-template-columns: repeat(3, 1fr);
        gap: 12px;
      }
      .wide {
        grid-column: span 2;
      }
      .package {
        display: grid;
        grid-template-columns: 110px 2fr repeat(3, 1fr) 40px;
        gap: 8px;
        margin: 8px 0;
      }
      .package input {
        padding: 9px;
        border: 1px solid #bbb;
        border-radius: 5px;
      }
      footer {
        display: flex;
        justify-content: flex-end;
        gap: 10px;
        background: #fff;
        padding: 16px;
      }
      @media (max-width: 800px) {
        .facts,
        .grid {
          grid-template-columns: 1fr;
        }
        .wide {
          grid-column: auto;
        }
        .package {
          grid-template-columns: 1fr 1fr;
        }
      }
    `,
  ],
})
export class DispatchFormComponent implements OnInit {
  summary = signal<DispatchSummary | null>(null);
  customers = signal<readonly Customer[]>([]);
  packages = signal<DispatchPackage[]>([]);
  private fb = inject(FormBuilder);
  private api = inject(DispatchService);
  private sales = inject(SalesService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private snack = inject(MatSnackBar);
  private workOrderId = '';
  form = this.fb.group({
    customerId: ['', Validators.required],
    dispatchQuantity: [0, [Validators.required, Validators.min(0.001)]],
    dispatchDate: [new Date().toISOString().slice(0, 10)],
    shippingAddress: ['', Validators.required],
    contactPerson: [''],
    contactPhone: [''],
    netWeight: [null as number | null],
    grossWeight: [null as number | null],
    transporterName: [''],
    vehicleNumber: [''],
    lrgrNumber: [''],
    eWayBillNumber: [''],
    packingRemarks: [''],
    dispatchRemarks: [''],
  });
  ngOnInit() {
    this.workOrderId =
      this.route.snapshot.queryParamMap.get('workOrderId') ?? '';
    if (!this.workOrderId) {
      this.router.navigate(['/work-orders']);
      return;
    }
    forkJoin({
      summary: this.api.summary(this.workOrderId),
      workOrder: this.api.workOrder(this.workOrderId),
      customers: this.sales.customers({ isActive: true, pageSize: 100 }),
    }).subscribe(({ summary, workOrder, customers }) => {
      this.summary.set(summary);
      this.customers.set(customers.data);
      const customer = customers.data.find((x) => x.id === workOrder.customerId)
        ?? customers.data.find((x) => x.customerName === workOrder.customerName)
        ?? customers.data[0];
      this.form.patchValue({
        dispatchQuantity: summary.availableForDispatchQuantity,
        customerId: customer?.id ?? '',
      });
      if (customer) this.customerChanged(customer.id);
    });
  }
  customerChanged(id: string) {
    const customer = this.customers().find((x) => x.id === id);
    if (!customer) return;
    this.form.patchValue({
      shippingAddress: customer.shippingAddress || customer.billingAddress,
      contactPerson: customer.contactPerson,
      contactPhone: customer.phone,
    });
  }
  add() {
    const p = [
      ...this.packages(),
      {
        packageNumber: `PKG-${this.packages().length + 1}`.replace(
          /(\d+)$/,
          (x) => x.padStart(2, '0'),
        ),
        description: '',
        quantity: undefined,
        quantityUnit: this.summary()?.quantityUnit,
        sequence: this.packages().length + 1,
      },
    ];
    this.packages.set(p);
  }
  remove(i: number) {
    this.packages.set(this.packages().filter((_, x) => x !== i));
  }
  setPackage(i: number, field: keyof DispatchPackage, event: Event, numeric = false) {
    const value = (event.target as HTMLInputElement).value;
    this.packages.update((items) =>
      items.map((item, index) =>
        index === i
          ? { ...item, [field]: numeric ? (value === '' ? undefined : Number(value)) : value }
          : item,
      ),
    );
  }
  save() {
    const r = {
      ...this.form.getRawValue(),
      packages: this.packages(),
      rowVersion: null,
    };
    this.api.create(this.workOrderId, r).subscribe({
      next: (x) => {
        this.snack.open(`Draft saved. Packing Slip ${x.packingSlipNumber} generated.`, 'Close', { duration: 4000 });
        this.router.navigate(['/dispatch', x.id]);
      },
      error: (e) =>
        this.snack.open(e?.error?.message || e.message, 'Close', {
          duration: 6000,
        }),
    });
  }
  unit(x: number) {
    return ['kg', 'pieces', 'sets'][x];
  }
}
