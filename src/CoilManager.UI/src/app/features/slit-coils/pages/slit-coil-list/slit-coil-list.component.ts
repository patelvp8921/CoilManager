import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSort, MatSortModule, Sort } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { CoilStatus, statusLabel } from '../../../raw-coil/models/raw-coil.model';
import { SlitCoil, SlitCoilQuery } from '../../models/slit-coil.model';
import { SlitCoilService } from '../../services/slit-coil.service';

@Component({
  selector: 'app-slit-coil-list',
  imports: [
    DatePipe,
    DecimalPipe,
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
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
    'width',
    'weight',
    'status',
    'warehouseLocation',
    'createdOn',
    'actions',
  ];
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
  protected readonly slitCoils = signal<readonly SlitCoil[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly pageSize = signal(25);
  protected readonly pageIndex = signal(0);
  protected readonly isLoading = signal(false);

  private readonly slitCoilService = inject(SlitCoilService);
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

  protected placeholder(action: string): void {
    this.snackBar.open(`${action} will be available in a later sprint.`, 'Close', { duration: 3000 });
  }

  protected loadSlitCoils(sort: Sort | null = this.sort ?? null): void {
    const query: SlitCoilQuery = {
      page: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      search: this.searchControl.value.trim(),
      status: this.statusControl.value,
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
