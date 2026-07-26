import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { productLabels, statusLabels, typeLabels, WorkOrderListItem } from './work-order.model';
import { WorkOrderService } from './work-order.service';

@Component({
  selector: 'app-work-order-list',
  imports: [
    DatePipe,
    DecimalPipe,
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressBarModule,
    MatSelectModule,
    MatTableModule,
  ],
  templateUrl: './work-order-list.component.html',
  styleUrl: './work-order-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WorkOrderListComponent implements OnInit {
  protected readonly typeLabels = typeLabels;
  protected readonly productLabels = productLabels;
  protected readonly statusLabels = statusLabels;
  protected readonly columns = ['number', 'type', 'product', 'customer', 'required', 'priority', 'status', 'progress', 'created', 'actions'];
  protected readonly rows = signal<readonly WorkOrderListItem[]>([]);
  protected readonly search = new FormControl('', { nonNullable: true });
  protected readonly type = new FormControl<number | null>(null);
  protected readonly product = new FormControl<number | null>(null);
  protected readonly status = new FormControl<number | null>(null);
  private readonly service = inject(WorkOrderService);
  private readonly route = inject(ActivatedRoute);

  ngOnInit(): void {
    this.status.setValue(this.statusFromQuery());
    this.load();
  }

  private statusFromQuery(): number | null {
    const rawStatus = this.route.snapshot.queryParamMap.get('status');
    if (rawStatus === null) return null;
    const value = Number(rawStatus);
    return Number.isInteger(value) && value >= 0 && value < this.statusLabels.length ? value : null;
  }

  protected load(): void {
    this.service.list({
      page: 1,
      pageSize: 100,
      search: this.search.value,
      workOrderType: this.type.value,
      productType: this.product.value,
      status: this.status.value,
    }).subscribe((response) => this.rows.set(response.data ?? []));
  }

  protected resetFilters(): void {
    this.search.setValue('');
    this.type.setValue(null);
    this.product.setValue(null);
    this.status.setValue(null);
    this.load();
  }
}
