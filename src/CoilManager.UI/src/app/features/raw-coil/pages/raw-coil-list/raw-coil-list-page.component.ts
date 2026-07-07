import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSort, MatSortModule, Sort } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { DeleteConfirmDialogComponent } from '../../components/delete-confirm-dialog/delete-confirm-dialog.component';
import { COIL_STATUS_OPTIONS, CoilStatus, RawCoil, statusLabel } from '../../models/raw-coil.model';
import { RawCoilQuery } from '../../models/raw-coil-query.model';
import { RawCoilService } from '../../services/raw-coil.service';

@Component({
  selector: 'app-raw-coil-list-page',
  imports: [
    DatePipe,
    DecimalPipe,
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatDialogModule,
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
  templateUrl: './raw-coil-list-page.component.html',
  styleUrl: './raw-coil-list-page.component.scss',
})
export class RawCoilListPageComponent implements OnInit {
  @ViewChild(MatPaginator) private paginator?: MatPaginator;
  @ViewChild(MatSort) private sort?: MatSort;

  protected readonly statusOptions = COIL_STATUS_OPTIONS;
  protected readonly displayedColumns = [
    'rawCoilNumber',
    'supplierName',
    'manufacturerName',
    'grade',
    'thickness',
    'width',
    'weight',
    'status',
    'receivedDate',
    'actions',
  ];

  protected readonly searchControl = new FormControl('', { nonNullable: true });
  protected readonly gradeControl = new FormControl('', { nonNullable: true });
  protected readonly manufacturerControl = new FormControl('', { nonNullable: true });
  protected readonly statusControl = new FormControl<CoilStatus | null>(null);

  protected readonly rawCoils = signal<readonly RawCoil[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly pageSize = signal(25);
  protected readonly pageIndex = signal(0);
  protected readonly isLoading = signal(false);

  private readonly rawCoilService = inject(RawCoilService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  ngOnInit(): void {
    this.loadRawCoils();
  }

  protected applyFilters(): void {
    this.pageIndex.set(0);
    this.paginator?.firstPage();
    this.loadRawCoils();
  }

  protected resetFilters(): void {
    this.searchControl.reset('');
    this.gradeControl.reset('');
    this.manufacturerControl.reset('');
    this.statusControl.reset(null);
    this.pageIndex.set(0);
    this.sort?.sort({ id: '', start: 'asc', disableClear: false });
    this.paginator?.firstPage();
    this.loadRawCoils();
  }

  protected onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.loadRawCoils();
  }

  protected onSortChange(sort: Sort): void {
    this.pageIndex.set(0);
    this.paginator?.firstPage();
    this.loadRawCoils(sort);
  }

  protected deleteRawCoil(rawCoil: RawCoil): void {
    const ref = this.dialog.open(DeleteConfirmDialogComponent, {
      width: '420px',
      data: { coilNumber: rawCoil.coilNumber },
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }

      this.isLoading.set(true);
      this.rawCoilService
        .deleteRawCoil(rawCoil.id)
        .pipe(finalize(() => this.isLoading.set(false)))
        .subscribe({
          next: () => {
            this.snackBar.open('Raw coil deleted.', 'Close', { duration: 3000 });
            this.loadRawCoils();
          },
          error: (error: HttpErrorResponse) => this.showError(error),
        });
    });
  }

  protected labelForStatus(status: CoilStatus): string {
    return statusLabel(status);
  }

  protected loadRawCoils(sort: Sort | null = this.sort ?? null): void {
    const query: RawCoilQuery = {
      page: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      search: this.searchControl.value.trim(),
      grade: this.gradeControl.value.trim(),
      manufacturer: this.manufacturerControl.value.trim(),
      status: this.statusControl.value,
      sortBy: sort?.active,
      sortDirection: sort?.direction,
    };

    this.isLoading.set(true);
    this.rawCoilService
      .getRawCoils(query)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          this.rawCoils.set(response.data);
          this.totalCount.set(response.pagination.totalCount);
        },
        error: (error: HttpErrorResponse) => this.showError(error),
      });
  }

  private showError(error: HttpErrorResponse): void {
    const message = this.extractError(error);
    this.snackBar.open(message, 'Close', { duration: 6000 });
  }

  private extractError(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'The API is not reachable at http://localhost:5170. Start CoilManager.API and try again.';
    }

    const body = error.error as { message?: string; errors?: string[] } | null;
    return body?.errors?.join('\n') || body?.message || error.message || 'Request failed.';
  }
}
