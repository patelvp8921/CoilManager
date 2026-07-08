import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSort, MatSortModule, Sort } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize, timeout } from 'rxjs';
import { MasterDataService } from './master-data.service';
import { MasterRecord, MasterRouteData } from './master-data.model';

@Component({
  selector: 'app-master-list-page',
  imports: [
    DatePipe,
    DecimalPipe,
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatInputModule,
    MatPaginatorModule,
    MatProgressBarModule,
    MatSelectModule,
    MatSnackBarModule,
    MatSortModule,
    MatTableModule,
  ],
  templateUrl: './master-list-page.component.html',
  styleUrl: './master-list-page.component.scss',
})
export class MasterListPageComponent implements OnInit {
  @ViewChild(MatPaginator) private paginator?: MatPaginator;
  @ViewChild(MatSort) private sort?: MatSort;

  protected readonly searchControl = new FormControl('', { nonNullable: true });
  protected readonly statusControl = new FormControl<boolean | null>(null);
  protected readonly items = signal<readonly MasterRecord[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly routeData = inject(ActivatedRoute).snapshot.data as MasterRouteData;
  protected readonly isManufacturer = this.routeData.type === 'manufacturers';
  protected readonly isSupplier = this.routeData.type === 'suppliers';
  protected readonly isGrade = this.routeData.type === 'grades';
  protected readonly displayedColumns = this.isSupplier
    ? ['name', 'address', 'gst', 'email', 'contactNo', 'isActive', 'createdOn', 'actions']
    : this.isManufacturer
      ? ['code', 'name', 'country', 'description', 'isActive', 'createdOn', 'actions']
      : ['grade', 'thicknessMm', 'category', 'coreLossPerKg', 'isActive', 'createdOn', 'actions'];
  protected totalCount = 0;
  protected pageSize = 25;
  protected pageIndex = 0;

  private readonly service = inject(MasterDataService);
  private readonly snackBar = inject(MatSnackBar);

  ngOnInit(): void {
    this.loadItems();
  }

  protected applyFilters(): void {
    this.pageIndex = 0;
    this.paginator?.firstPage();
    this.loadItems();
  }

  protected resetFilters(): void {
    this.searchControl.reset('');
    this.statusControl.reset(null);
    this.pageIndex = 0;
    this.sort?.sort({ id: '', start: 'asc', disableClear: false });
    this.paginator?.firstPage();
    this.loadItems();
  }

  protected onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadItems();
  }

  protected onSortChange(sort: Sort): void {
    this.pageIndex = 0;
    this.paginator?.firstPage();
    this.loadItems(sort);
  }

  protected toggleStatus(item: MasterRecord): void {
    this.isLoading.set(true);
    this.service
      .setActive(this.routeData.type, item.id, !item.isActive)
      .pipe(
        timeout(15000),
        finalize(() => this.isLoading.set(false)),
      )
      .subscribe({
        next: () => {
          this.snackBar.open(`${this.routeData.singular} ${item.isActive ? 'deactivated' : 'activated'}.`, 'Close', { duration: 3000 });
          this.loadItems();
        },
        error: (error: HttpErrorResponse) => this.showError(error),
      });
  }

  protected loadItems(sort: Sort | null = this.sort ?? null): void {
    this.isLoading.set(true);
    this.service
      .getAll(this.routeData.type, {
        page: this.pageIndex + 1,
        pageSize: this.pageSize,
        search: this.searchControl.value.trim(),
        isActive: this.statusControl.value,
        sortBy: sort?.active,
        sortDirection: sort?.direction,
      })
      .pipe(
        timeout(15000),
        finalize(() => this.isLoading.set(false)),
      )
      .subscribe({
        next: (response) => {
          this.items.set(response.data);
          this.totalCount = response.pagination.totalCount;
        },
        error: (error: HttpErrorResponse) => this.showError(error),
      });
  }

  private showError(error: HttpErrorResponse): void {
    const body = error.error as { message?: string; errors?: string[] } | null;
    this.snackBar.open(body?.errors?.join('\n') || body?.message || error.message || 'Request failed.', 'Close', { duration: 6000 });
  }
}
