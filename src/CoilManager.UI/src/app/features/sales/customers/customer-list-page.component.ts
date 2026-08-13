import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { Customer } from '../sales.model';
import { SalesService } from '../sales.service';

@Component({
  selector: 'app-customer-list-page',
  imports: [DatePipe, ReactiveFormsModule, RouterLink, MatButtonModule, MatCardModule, MatFormFieldModule,
    MatIconModule, MatInputModule, MatPaginatorModule, MatProgressBarModule, MatSelectModule, MatTableModule],
  templateUrl: './customer-list-page.component.html',
  styleUrl: './customer-list-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CustomerListPageComponent implements OnInit {
  @ViewChild(MatPaginator) private paginator?: MatPaginator;
  protected readonly columns = ['code', 'name', 'location', 'contact', 'gst', 'terms', 'isActive', 'createdOn', 'actions'];
  protected readonly rows = signal<readonly Customer[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly search = new FormControl('', { nonNullable: true });
  protected readonly active = new FormControl<boolean | null>(null);
  protected totalCount = 0;
  protected pageSize = 25;
  protected pageIndex = 0;
  private readonly service = inject(SalesService);
  private readonly snackBar = inject(MatSnackBar);

  ngOnInit(): void { this.load(); }
  protected applyFilters(): void { this.pageIndex = 0; this.paginator?.firstPage(); this.load(); }
  protected reset(): void { this.search.reset(''); this.active.reset(null); this.pageIndex = 0; this.paginator?.firstPage(); this.load(); }
  protected refresh(): void { this.load(); }
  protected pageChanged(event: PageEvent): void { this.pageIndex = event.pageIndex; this.pageSize = event.pageSize; this.load(); }
  protected toggle(customer: Customer): void {
    this.isLoading.set(true);
    this.service.setCustomerActive(customer.id, !customer.isActive).subscribe({
      next: () => {
        this.snackBar.open(`Customer ${customer.isActive ? 'deactivated' : 'activated'}.`, 'Close', { duration: 2500 });
        this.load();
      },
      error: () => this.isLoading.set(false),
    });
  }
  protected load(): void {
    this.isLoading.set(true);
    this.service.customers({ search: this.search.value, isActive: this.active.value, page: this.pageIndex + 1, pageSize: this.pageSize })
      .subscribe({ next: response => { this.rows.set(response.data); this.totalCount = response.pagination.totalCount; this.isLoading.set(false); }, error: () => this.isLoading.set(false) });
  }
}
