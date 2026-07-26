import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSort, MatSortModule, Sort } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';
import { DashboardService } from '../../../dashboard/services/dashboard.service';
import { CoilStatus, statusLabel } from '../../../raw-coil/models/raw-coil.model';
import { SlitCoil, SlitCoilQuery } from '../../models/slit-coil.model';
import { SlitCoilService } from '../../services/slit-coil.service';

@Component({
  selector: 'app-slit-coil-list',
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
    MatMenuModule,
    MatPaginatorModule,
    MatProgressBarModule,
    MatSelectModule,
    MatSnackBarModule,
    MatSortModule,
    MatTableModule,
  ],
  templateUrl: './slit-coil-list.component.html',
  styleUrl: './slit-coil-list.component.scss',
})
export class SlitCoilListComponent implements OnInit {
  @ViewChild(MatPaginator) private paginator?: MatPaginator;
  @ViewChild(MatSort) private sort?: MatSort;

  protected readonly displayedColumns = [
    'coilNumber',
    'motherCoilNumber',
    'slittingJobNo',
    'grade',
    'thickness',
    'category',
    'width',
    'weight',
    'manufacturer',
    'status',
    'warehouseLocation',
    'createdOn',
    'actions',
  ];
  protected readonly CoilStatus = CoilStatus;
  protected readonly statusOptions = [
    CoilStatus.Available,
    CoilStatus.Reserved,
    CoilStatus.Consumed,
    CoilStatus.OnHold,
    CoilStatus.Rejected,
    CoilStatus.Dispatched,
  ];

  protected readonly searchControl = new FormControl('', { nonNullable: true });
  protected readonly statusControl = new FormControl<CoilStatus | null>(null);
  protected readonly gradeControl = new FormControl('', { nonNullable: true });
  protected readonly widthFromControl = new FormControl<number | null>(null);
  protected readonly widthToControl = new FormControl<number | null>(null);
  protected readonly slitCoils = signal<readonly SlitCoil[]>([]);
  protected readonly totalWeight = signal(0);
  protected readonly totalCount = signal(0);
  protected readonly pageSize = signal(25);
  protected readonly pageIndex = signal(0);
  protected readonly isLoading = signal(false);

  private readonly slitCoilService = inject(SlitCoilService);
  private readonly dashboardService = inject(DashboardService);
  private readonly snackBar = inject(MatSnackBar);

  ngOnInit(): void {
    this.loadSlitCoils();
  }

  protected labelForStatus(status: CoilStatus): string {
    return statusLabel(status);
  }

  protected applyFilters(): void {
    this.pageIndex.set(0);
    this.paginator?.firstPage();
    this.loadSlitCoils();
  }

  protected resetFilters(): void {
    this.searchControl.reset('');
    this.statusControl.reset(null);
    this.gradeControl.reset(''); this.widthFromControl.reset(null); this.widthToControl.reset(null);
    this.pageIndex.set(0);
    this.paginator?.firstPage();
    this.loadSlitCoils();
  }

  protected onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.loadSlitCoils();
  }

  protected onSortChange(sort: Sort): void {
    this.pageIndex.set(0);
    this.paginator?.firstPage();
    this.loadSlitCoils(sort);
  }

  private loadInventoryTotal(): void {
    this.dashboardService.getOperationsDashboard().subscribe({
      next: (dashboard) => this.totalWeight.set(dashboard.inventory.totalSlitWeight),
      error: (error: HttpErrorResponse) => {
        const body = error.error as { message?: string; errors?: string[] } | null;
        this.snackBar.open(body?.errors?.join('\\n') || body?.message || error.message, 'Close', { duration: 6000 });
      },
    });
  }

  protected loadSlitCoils(sort: Sort | null = this.sort ?? null): void {
    this.loadInventoryTotal();
    const query: SlitCoilQuery = {
      page: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      status: this.statusControl.value,
      search: [this.searchControl.value, this.gradeControl.value].filter(Boolean).join(' ').trim(),
      widthFrom: this.widthFromControl.value, widthTo: this.widthToControl.value,
      sortBy: sort?.active,
      sortDirection: sort?.direction,
    };

    this.isLoading.set(true);
    this.slitCoilService.getSlitCoils(query).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        this.slitCoils.set(response.data);
        this.totalCount.set(response.pagination.totalCount);
      },
      error: (error: HttpErrorResponse) => {
        this.isLoading.set(false);
        const body = error.error as { message?: string; errors?: string[] } | null;
        this.snackBar.open(body?.errors?.join('\n') || body?.message || error.message, 'Close', { duration: 6000 });
      },
    });
  }
}
