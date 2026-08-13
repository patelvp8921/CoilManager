import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  operationLabels,
  operationStatusLabels,
  productLabels,
  statusLabels,
  typeLabels,
  WorkOrder,
  WorkOrderNextAction,
} from './work-order.model';
import { WorkOrderService } from './work-order.service';
import { WorkOrderAllocationDialogComponent } from './work-order-allocation-dialog.component';

@Component({
  selector: 'app-work-order-detail',
  imports: [
    DatePipe,
    DecimalPipe,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatDialogModule,
    MatIconModule,
    MatProgressBarModule,
    MatSnackBarModule,
  ],
  template: `
    @if (wo(); as x) {
      <section class="page-title-bar">
        <div>
          <h1>View Work Order</h1>
          <nav>
            <a routerLink="/dashboard">Home</a><mat-icon>chevron_right</mat-icon
            ><a routerLink="/work-orders">Work Orders</a
            ><mat-icon>chevron_right</mat-icon><strong>View</strong>
          </nav>
        </div>
        <div class="header-actions">
          <span class="status-badge s{{ x.status }}">{{
            statusLabels[x.status]
          }}</span>
          @if (x.status === 0) {
            <a mat-stroked-button [routerLink]="['/work-orders', x.id, 'edit']"
              ><mat-icon>edit</mat-icon>Edit</a
            ><button mat-flat-button (click)="action('release')">
              <mat-icon>send</mat-icon>Release</button
            ><button mat-button class="danger" (click)="action('cancel')">
              Cancel
            </button>
          }
          <a mat-stroked-button routerLink="/work-orders"
            ><mat-icon>arrow_back</mat-icon>Back</a
          >
        </div>
      </section>
      <section class="form-page two-column-layout">
        <main class="entry-column">
          <mat-card appearance="outlined"
            ><div class="form-body">
              <section class="form-section next-actions">
                <header>
                  <div class="section-icon"><mat-icon>task_alt</mat-icon></div>
                  <div>
                    <h2>Next Actions</h2>
                    <span
                      >Recommended execution steps calculated by the
                      server.</span
                    >
                  </div>
                </header>
                @for (action of nextActions(); track action.key) {
                  <div class="next-action">
                    <div>
                      <strong>{{ action.title }}</strong>
                      <p>{{ action.description }}</p>
                      <span>
                        @if (action.plannedQuantity != null) {
                          Planned {{ action.plannedQuantity }} ·
                        }
                        @if (action.completedQuantity != null) {
                          Completed {{ action.completedQuantity }} ·
                        }
                        @if (action.remainingQuantity != null) {
                          Remaining {{ action.remainingQuantity }}
                        }
                      </span>
                    </div>
                    @if (action.actionCode === 'VIEW_INVENTORY_ALLOCATION') {
                      <button
                        mat-flat-button
                        [disabled]="!action.isEnabled"
                        (click)="viewAllocations()"
                      >
                        {{ action.actionLabel }}
                      </button>
                    } @else if (action.actionCode === 'CREATE_DISPATCH') {
                      <a
                        mat-flat-button
                        [routerLink]="['/dispatch-create']"
                        [queryParams]="{ workOrderId: id }"
                        [disabled]="!action.isEnabled"
                        >{{ action.actionLabel }}</a
                      >
                    } @else if (action.route && action.actionLabel) {
                      <a
                        mat-flat-button
                        [routerLink]="action.route"
                        [disabled]="!action.isEnabled"
                        >{{ action.actionLabel }}</a
                      >
                    }
                    @if (
                      !action.route &&
                      action.actionLabel &&
                      (action.actionCode === 'RELEASE_WORK_ORDER' ||
                        action.actionCode === 'RECOVER_LAMINATION_JOB' ||
                        action.actionCode === 'COMPLETE_WORK_ORDER')
                    ) {
                      <button
                        mat-flat-button
                        [disabled]="!action.isEnabled"
                        (click)="runNextAction(action.actionCode)"
                      >
                        {{ action.actionLabel }}
                      </button>
                    }
                    @if (!action.isEnabled && action.disabledReason) {
                      <small>{{ action.disabledReason }}</small>
                    }
                  </div>
                }
              </section>
              <section class="form-section">
                <header>
                  <div class="section-icon">
                    <mat-icon>assignment</mat-icon>
                  </div>
                  <div>
                    <h2>Work Order Identity</h2>
                    <span>Document number, planning dates, and priority.</span>
                  </div>
                </header>
                <div class="form-grid">
                  <div class="display-field">
                    <span>Work Order Number</span
                    ><strong>{{ x.workOrderNumber }}</strong>
                  </div>
                  <div class="display-field">
                    <span>Work Order Date</span
                    ><strong>{{
                      x.workOrderDate | date: 'dd MMM yyyy'
                    }}</strong>
                  </div>
                  <div class="display-field">
                    <span>Required Date</span
                    ><strong>{{
                      x.requiredDate
                        ? (x.requiredDate | date: 'dd MMM yyyy')
                        : '—'
                    }}</strong>
                  </div>
                  <div class="display-field">
                    <span>Priority</span><strong>P{{ x.priority }}</strong>
                  </div>
                </div>
              </section>
              <section class="form-section">
                <header>
                  <div class="section-icon"><mat-icon>source</mat-icon></div>
                  <div>
                    <h2>Order Source</h2>
                    <span
                      >Customer demand, inventory production, rework, or
                      trial.</span
                    >
                  </div>
                </header>
                <div class="form-grid">
                  <div class="display-field">
                    <span>Work Order Type</span
                    ><strong>{{ typeLabels[x.workOrderType] }}</strong>
                  </div>
                  <div class="display-field">
                    <span>Customer Name</span
                    ><strong>{{ x.customerName || '—' }}</strong>
                  </div>
                  <div class="display-field">
                    <span>Sales Order Reference</span
                    ><strong>{{ x.salesOrderReference || '—' }}</strong>
                  </div>
                </div>
              </section>
              <section class="form-section">
                <header>
                  <div class="section-icon">
                    <mat-icon>inventory_2</mat-icon>
                  </div>
                  <div>
                    <h2>Product Requirement</h2>
                    <span
                      >Product, dimensions, target weight, and quantity.</span
                    >
                  </div>
                </header>
                <div class="form-grid">
                  <div class="display-field">
                    <span>Product Type</span
                    ><strong>{{ productLabels[x.productType] }}</strong>
                  </div>
                  <div class="display-field">
                    <span>Required Width</span
                    ><strong>{{
                      x.requiredWidth != null ? x.requiredWidth + ' mm' : '—'
                    }}</strong>
                  </div>
                  <div class="display-field">
                    <span>Required Weight</span
                    ><strong>{{
                      x.requiredWeight != null
                        ? (x.requiredWeight | number: '1.0-3') + ' kg'
                        : '—'
                    }}</strong>
                  </div>
                  <div class="display-field">
                    <span>Required Quantity</span
                    ><strong>{{ x.requiredQuantity || '—' }}</strong>
                  </div>
                  <div class="display-field">
                    <span>Drawing Number</span
                    ><strong>{{ x.drawingNumber || '—' }}</strong>
                  </div>
                </div>
              </section>
              <section class="form-section">
                <header>
                  <div class="section-icon"><mat-icon>category</mat-icon></div>
                  <div>
                    <h2>Material Specification</h2>
                    <span>Grade and electrical steel specification.</span>
                  </div>
                </header>
                <div class="form-grid">
                  <div class="display-field">
                    <span>Grade</span
                    ><strong>{{ x.grade || 'Not specified' }}</strong>
                  </div>
                  <div class="display-field">
                    <span>Thickness</span><strong>{{ x.thickness }} mm</strong>
                  </div>
                  <div class="display-field">
                    <span>Category</span
                    ><strong>{{ x.category || '—' }}</strong>
                  </div>
                  <div class="display-field">
                    <span>Core Loss / kg</span
                    ><strong>{{ x.coreLossPerKg }}</strong>
                  </div>
                </div>
              </section>
              <section class="form-section">
                <header>
                  <div class="section-icon">
                    <mat-icon>precision_manufacturing</mat-icon>
                  </div>
                  <div>
                    <h2>Production</h2>
                    <span>Product-specific fulfilment workflow.</span>
                  </div>
                </header>
                @if (x.productType === 0) {
                  <div class="remarks">
                    <strong>No production operation required.</strong><br />This
                    Work Order is fulfilled directly from Mother Coil inventory.
                  </div>
                }
                @if (x.productType === 1) {
                  <div class="remarks">
                    <strong
                      >No production operation required for this Work
                      Order.</strong
                    ><br />This Work Order is fulfilled from existing Slit Coil
                    inventory. Any shortage is handled independently through the
                    Slitting Jobs module.
                  </div>
                }
                @if (x.productType === 2) {
                  @if (x.linkedLaminationJob; as job) {
                    <div class="next-action">
                      <div>
                        <strong>{{ job.laminationJobNumber }}</strong>
                        <p>
                          Drawing {{ job.drawingNumber || '—' }} · Required
                          {{ job.requiredQuantity }} · Material Allocation
                          {{
                            job.materialAllocationPercentage | number: '1.0-1'
                          }}%
                        </p>
                        <span
                          >Created {{ job.createdOn | date: 'dd MMM yyyy' }} ·
                          Status {{ job.status }}</span
                        >
                      </div>
                      <a
                        mat-flat-button
                        [routerLink]="['/lamination-jobs', job.id]"
                        >View Lamination Job</a
                      >
                    </div>
                  } @else {
                    <div class="remarks">
                      The Draft Lamination Job will be created automatically
                      when this Work Order is released.
                    </div>
                  }
                }
              </section>
              <section class="form-section">
                <header>
                  <div class="section-icon"><mat-icon>notes</mat-icon></div>
                  <div>
                    <h2>Remarks</h2>
                    <span>Planning and production instructions.</span>
                  </div>
                </header>
                <div class="remarks">{{ x.remarks || 'No remarks.' }}</div>
              </section>
            </div>
            <div class="actions">
              <a mat-stroked-button routerLink="/work-orders"
                >Back to Work Orders</a
              >
              @if (x.status === 0) {
                <a mat-flat-button [routerLink]="['/work-orders', x.id, 'edit']"
                  ><mat-icon>edit</mat-icon>Edit Work Order</a
                >
              }
            </div></mat-card
          >
        </main>
        <aside class="summary-column">
          <mat-card appearance="outlined" class="summary-card"
            ><div class="summary-header">
              <div class="summary-icon"><mat-icon>assignment</mat-icon></div>
              <div>
                <span>Work Order Summary</span
                ><strong>{{ x.workOrderNumber }}</strong>
              </div>
            </div>
            <dl>
              <div>
                <dt>Status</dt>
                <dd>{{ statusLabels[x.status] }}</dd>
              </div>
              <div>
                <dt>Source</dt>
                <dd>{{ typeLabels[x.workOrderType] }}</dd>
              </div>
              <div>
                <dt>Product</dt>
                <dd>{{ productLabels[x.productType] }}</dd>
              </div>
              <div>
                <dt>Grade</dt>
                <dd>{{ x.grade || 'Not specified' }}</dd>
              </div>
              <div>
                <dt>Requirement</dt>
                <dd>
                  {{ x.requiredWeight || 0 | number: '1.0-3' }} kg /
                  {{ x.requiredQuantity || 0 }} units
                </dd>
              </div>
              <div>
                <dt>Priority</dt>
                <dd>
                  <span class="priority-badge">P{{ x.priority }}</span>
                </dd>
              </div>
            </dl></mat-card
          ><mat-card appearance="outlined" class="help-card"
            ><mat-icon>visibility</mat-icon>
            <div>
              <strong>Display only</strong>
              <p>
                This page shows the saved Work Order values. Use Edit Work Order
                to make changes while it remains in Draft.
              </p>
            </div></mat-card
          >
        </aside>
      </section>
    }
  `,
  styles: [
    `
      :host {
        display: block;
        min-height: 100%;
        background: #f5f7fa;
      }
      .page-title-bar {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 18px;
        border-bottom: 1px solid #dde4ee;
        padding: 24px 32px;
        background: #fff;
      }
      .page-title-bar h1 {
        margin: 0;
        color: #111827;
        font-size: 26px;
      }
      .page-title-bar nav {
        display: flex;
        align-items: center;
        gap: 8px;
        margin-top: 8px;
        color: #667085;
        font-size: 13px;
      }
      .page-title-bar nav a,
      .page-title-bar nav strong {
        color: #005eef;
        text-decoration: none;
      }
      .page-title-bar nav mat-icon {
        width: 16px;
        height: 16px;
        font-size: 16px;
      }
      .header-actions {
        display: flex;
        align-items: center;
        flex-wrap: wrap;
        gap: 9px;
      }
      .header-actions a,
      .header-actions button {
        border-radius: 6px;
      }
      .danger {
        color: #b42318;
      }
      .status-badge {
        border-radius: 999px;
        padding: 7px 14px;
        background: #fff4d6;
        color: #8a5b00;
        font-size: 13px;
        font-weight: 700;
      }
      .s1 {
        background: #ecfdf3;
        color: #027a48;
      }
      .s5,
      .s7 {
        background: #fef3f2;
        color: #b42318;
      }
      .form-page {
        padding: 22px;
      }
      .two-column-layout {
        display: grid;
        grid-template-columns: minmax(0, 7fr) minmax(290px, 3fr);
        gap: 18px;
        align-items: start;
      }
      .entry-column {
        min-width: 0;
      }
      .entry-column > mat-card {
        padding: 0;
        overflow: hidden;
      }
      .form-body {
        display: grid;
        gap: 16px;
        padding: 18px;
      }
      .form-section {
        border: 1px solid #e2e8f0;
        border-radius: 6px;
        padding: 16px;
        background: #fff;
      }
      .form-section > header {
        display: flex;
        align-items: center;
        gap: 11px;
        margin-bottom: 14px;
      }
      .section-icon {
        display: grid;
        flex: 0 0 34px;
        width: 34px;
        height: 34px;
        place-items: center;
        border-radius: 6px;
        background: #eaf2ff;
        color: #005eef;
      }
      .section-icon mat-icon {
        font-size: 19px;
        width: 19px;
        height: 19px;
      }
      .form-section h2 {
        margin: 0;
        color: #111827;
        font-size: 16px;
      }
      .form-section header span {
        display: block;
        margin-top: 3px;
        color: #667085;
        font-size: 12px;
      }
      .next-action {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 14px;
        border: 1px solid #dbe4ef;
        border-radius: 6px;
        padding: 13px;
        margin-top: 10px;
        background: #f8fafc;
      }
      .next-action p {
        margin: 4px 0;
        color: #667085;
        font-size: 12px;
      }
      .next-action span,
      .next-action small {
        color: #667085;
        font-size: 11px;
      }
      .form-grid {
        display: grid;
        grid-template-columns: repeat(3, minmax(180px, 1fr));
        gap: 14px;
      }
      .display-field {
        min-height: 55px;
        border: 1px solid #cbd5e1;
        border-radius: 5px;
        padding: 9px 12px;
        background: #f8fafc;
      }
      .display-field span {
        display: block;
        margin-bottom: 5px;
        color: #667085;
        font-size: 11px;
      }
      .display-field strong {
        color: #111827;
        font-size: 14px;
        font-weight: 600;
      }
      .route-grid {
        display: grid;
        grid-template-columns: repeat(3, 1fr);
        gap: 12px;
      }
      .route-card {
        position: relative;
        display: flex;
        align-items: center;
        gap: 11px;
        border: 1px solid #bfdbfe;
        border-radius: 6px;
        padding: 14px;
        background: #f8fbff;
        color: #005eef;
      }
      .route-card.not-required {
        border-color: #e2e8f0;
        background: #f8fafc;
        color: #94a3b8;
      }
      .route-card div {
        display: grid;
        gap: 3px;
      }
      .route-card strong {
        color: #111827;
        font-size: 14px;
      }
      .route-card span:not(.route-sequence) {
        font-size: 12px;
      }
      .route-sequence {
        position: absolute;
        top: 5px;
        right: 8px;
        color: #94a3b8;
        font-size: 11px;
        font-weight: 700;
      }
      .remarks {
        min-height: 70px;
        border: 1px solid #cbd5e1;
        border-radius: 5px;
        padding: 12px;
        background: #f8fafc;
        color: #334155;
        white-space: pre-wrap;
      }
      .actions {
        display: flex;
        justify-content: flex-end;
        gap: 10px;
        border-top: 1px solid #e2e8f0;
        padding: 14px 18px;
        background: #f8fafc;
      }
      .actions a {
        border-radius: 6px;
      }
      .summary-column {
        display: grid;
        position: sticky;
        top: 16px;
        gap: 16px;
        min-width: 0;
      }
      .summary-card,
      .help-card {
        padding: 18px;
      }
      .summary-header {
        display: flex;
        align-items: center;
        gap: 12px;
        border-bottom: 1px solid #e2e8f0;
        padding-bottom: 14px;
      }
      .summary-icon {
        display: grid;
        width: 42px;
        height: 42px;
        place-items: center;
        border-radius: 8px;
        background: #005eef;
        color: #fff;
      }
      .summary-header div:last-child {
        display: grid;
        gap: 3px;
      }
      .summary-header span {
        color: #667085;
        font-size: 12px;
      }
      .summary-header strong {
        color: #111827;
        font-size: 17px;
      }
      dl {
        display: grid;
        margin: 6px 0 0;
      }
      dl div {
        display: grid;
        grid-template-columns: 1fr 1.25fr;
        gap: 10px;
        border-bottom: 1px solid #eef2f6;
        padding: 12px 0;
      }
      dl div:last-child {
        border-bottom: 0;
      }
      dt {
        color: #667085;
        font-size: 13px;
      }
      dd {
        margin: 0;
        color: #111827;
        font-size: 13px;
        font-weight: 600;
        text-align: right;
      }
      .priority-badge {
        border-radius: 999px;
        padding: 3px 8px;
        background: #fff4d6;
        color: #8a5b00;
      }
      .help-card {
        display: flex;
        align-items: flex-start;
        gap: 12px;
        color: #334155;
      }
      .help-card > mat-icon {
        color: #005eef;
      }
      .help-card p {
        margin: 5px 0 0;
        color: #667085;
        font-size: 12px;
        line-height: 1.5;
      }
      @media (max-width: 1180px) {
        .two-column-layout {
          grid-template-columns: 1fr;
        }
        .summary-column {
          position: static;
        }
      }
      @media (max-width: 980px) {
        .form-grid {
          grid-template-columns: repeat(2, minmax(0, 1fr));
        }
      }
      @media (max-width: 680px) {
        .page-title-bar {
          align-items: flex-start;
          flex-direction: column;
          padding: 20px;
        }
        .form-page {
          padding: 14px;
        }
        .form-grid,
        .route-grid {
          grid-template-columns: 1fr;
        }
        .actions {
          flex-direction: column-reverse;
        }
      }
    `,
  ],
})
export class WorkOrderDetailComponent implements OnInit {
  protected readonly statusLabels = statusLabels;
  protected readonly typeLabels = typeLabels;
  protected readonly productLabels = productLabels;
  protected readonly operationLabels = operationLabels;
  protected readonly operationStatusLabels = operationStatusLabels;
  protected readonly wo = signal<WorkOrder | null>(null);
  protected readonly nextActions = signal<readonly WorkOrderNextAction[]>([]);
  private readonly service = inject(WorkOrderService);
  private readonly route = inject(ActivatedRoute);
  private readonly snack = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);
  protected id = '';
  ngOnInit() {
    this.id = this.route.snapshot.paramMap.get('id')!;
    this.service.get(this.id).subscribe({
      next: (workOrder) => {
        this.wo.set(workOrder);
        this.loadNextActions();
      },
      error: (error) =>
        this.snack.open(
          error?.error?.message ||
            error?.message ||
            'Unable to load Work Order.',
          'Close',
          { duration: 6000 },
        ),
    });
  }
  private loadNextActions() {
    this.service
      .nextActions(this.id)
      .subscribe({
        next: (actions) => this.nextActions.set(actions),
        error: () => this.nextActions.set([]),
      });
  }
  protected viewAllocations() {
    const workOrder = this.wo();
    if (!workOrder) return;
    this.dialog.open(WorkOrderAllocationDialogComponent, {
      width: '900px',
      maxWidth: '95vw',
      data: {
        workOrder,
        allocations: workOrder.allocations.filter((x) => x.status < 3),
      },
    });
  }
  protected runNextAction(code: string) {
    const name =
      code === 'RECOVER_LAMINATION_JOB'
        ? 'recover-lamination-job'
        : code === 'COMPLETE_WORK_ORDER'
          ? 'complete'
          : 'release';
    this.service.action(this.id, name).subscribe({
      next: (x) => {
        this.wo.set(x);
        this.loadNextActions();
        this.snack.open(
          name === 'release'
            ? 'Work Order released.'
            : name === 'complete'
              ? 'Work Order completed.'
              : 'Draft Lamination Job created.',
          'Close',
          { duration: 3000 },
        );
      },
      error: (e) =>
        this.snack.open(e?.error?.message || e.message, 'Close', {
          duration: 5000,
        }),
    });
  }
  protected action(name: string) {
    if (name === 'cancel' && !confirm('Cancel this Work Order?')) return;
    this.service.action(this.id, name).subscribe({
      next: (x) => {
        this.wo.set(x);
        this.loadNextActions();
        this.snack.open(`Work Order ${name}d.`, 'Close', { duration: 3000 });
      },
      error: (e) =>
        this.snack.open(e?.error?.message || e.message, 'Close', {
          duration: 5000,
        }),
    });
  }
}
