import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { priorityLabels, SalesOrder, SalesOrderPriority, SalesOrderStatus, statusLabels } from '../sales.model';
import { SalesService } from '../sales.service';

@Component({
  selector: 'app-sales-order-list-page',
  imports: [CurrencyPipe, DatePipe, ReactiveFormsModule, RouterLink, MatButtonModule, MatCardModule,
    MatFormFieldModule, MatIconModule, MatInputModule, MatMenuModule, MatPaginatorModule,
    MatProgressBarModule, MatSelectModule, MatTableModule],
  templateUrl: './sales-order-list-page.component.html',
  styleUrl: './sales-order-list-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SalesOrderListPageComponent implements OnInit {
  @ViewChild(MatPaginator) private paginator?: MatPaginator;
  protected readonly statusLabels = statusLabels;
  protected readonly priorityLabels = priorityLabels;
  protected readonly statuses = [SalesOrderStatus.Draft, SalesOrderStatus.Confirmed, SalesOrderStatus.OnHold, SalesOrderStatus.Cancelled];
  protected readonly priorities = [SalesOrderPriority.Low, SalesOrderPriority.Normal, SalesOrderPriority.High, SalesOrderPriority.Urgent];
  protected readonly columns = ['number', 'customer', 'poNumber', 'orderDate', 'deliveryDate', 'lines', 'quantity', 'value', 'priority', 'status', 'actions'];
  protected readonly rows = signal<readonly SalesOrder[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly search = new FormControl('', { nonNullable: true });
  protected readonly status = new FormControl<SalesOrderStatus | null>(null);
  protected readonly priority = new FormControl<SalesOrderPriority | null>(null);
  protected totalCount = 0;
  protected pageSize = 25;
  protected pageIndex = 0;
  private readonly service = inject(SalesService);
  private readonly snackBar = inject(MatSnackBar);

  ngOnInit(): void { this.load(); }
  protected applyFilters(): void { this.pageIndex = 0; this.paginator?.firstPage(); this.load(); }
  protected reset(): void { this.search.reset(''); this.status.reset(null); this.priority.reset(null); this.pageIndex = 0; this.paginator?.firstPage(); this.load(); }
  protected pageChanged(event: PageEvent): void { this.pageIndex = event.pageIndex; this.pageSize = event.pageSize; this.load(); }
  protected quantitySummary(order: SalesOrder): string {
    return [order.totalWeightKg ? `${order.totalWeightKg} kg` : '', order.totalPieces ? `${order.totalPieces} pcs` : '', order.totalSets ? `${order.totalSets} sets` : ''].filter(Boolean).join(' / ') || '-';
  }
  protected transition(order: SalesOrder, action: 'confirm' | 'hold' | 'release-hold'): void {
    this.isLoading.set(true);
    this.service.transition(order.id, action).subscribe({ next: () => { this.snackBar.open('Sales Order status updated.', 'Close', { duration: 2500 }); this.load(); }, error: () => this.isLoading.set(false) });
  }
  protected remove(order: SalesOrder): void { if (confirm(`Delete draft ${order.salesOrderNumber}?`)) { this.isLoading.set(true); this.service.deleteOrder(order.id).subscribe({ next: () => this.load(), error: () => this.isLoading.set(false) }); } }
  protected cancel(order: SalesOrder): void { const reason = prompt('Enter the cancellation reason:'); if (reason?.trim()) { this.isLoading.set(true); this.service.cancel(order.id, reason, order.rowVersion).subscribe({ next: () => this.load(), error: () => this.isLoading.set(false) }); } }
  protected load(): void {
    this.isLoading.set(true);
    this.service.orders({ search: this.search.value, status: this.status.value, priority: this.priority.value, page: this.pageIndex + 1, pageSize: this.pageSize })
      .subscribe({ next: response => { this.rows.set(response.data); this.totalCount = response.pagination.totalCount; this.isLoading.set(false); }, error: () => this.isLoading.set(false) });
  }
}
