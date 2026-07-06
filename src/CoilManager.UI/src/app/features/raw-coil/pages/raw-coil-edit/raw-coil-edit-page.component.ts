import { HttpErrorResponse } from '@angular/common/http';
import { Component, ElementRef, OnInit, inject } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize, forkJoin } from 'rxjs';
import { LookupItem } from '../../../../shared/models/lookup-item.model';
import { LookupService } from '../../../../shared/services/lookup.service';
import { COIL_STATUS_OPTIONS, CoilStatus, RawCoil, UpdateRawCoilRequest } from '../../models/raw-coil.model';
import { RawCoilService } from '../../services/raw-coil.service';

@Component({
  selector: 'app-raw-coil-edit-page',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressBarModule,
    MatSelectModule,
    MatSnackBarModule,
    MatTabsModule,
  ],
  templateUrl: './raw-coil-edit-page.component.html',
  styleUrl: '../raw-coil-create/raw-coil-create-page.component.scss',
})
export class RawCoilEditPageComponent implements OnInit {
  protected readonly statusOptions = COIL_STATUS_OPTIONS;
  protected readonly today = new Date().toISOString().slice(0, 10);
  protected isLoading = false;
  protected isSubmitting = false;
  protected apiErrors: readonly string[] = [];
  protected rawCoil?: RawCoil;
  protected suppliers: readonly LookupItem[] = [];
  protected manufacturers: readonly LookupItem[] = [];
  protected grades: readonly LookupItem[] = [];

  private readonly fb = inject(FormBuilder);
  private readonly elementRef = inject(ElementRef<HTMLElement>);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly lookupService = inject(LookupService);
  private readonly rawCoilService = inject(RawCoilService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly id = this.route.snapshot.paramMap.get('id') ?? '';

  protected readonly form = this.fb.nonNullable.group({
    coilNumber: ['', [Validators.required, Validators.maxLength(50)]],
    heatNumber: ['', [Validators.required, Validators.maxLength(50)]],
    poNumber: ['', [Validators.maxLength(50)]],
    invoiceNo: ['', [Validators.maxLength(50)]],
    millTCNo: ['', [Validators.maxLength(100)]],
    bisLicNumber: ['', [Validators.maxLength(100)]],
    supplierId: ['', [Validators.required]],
    manufacturerId: ['', [Validators.required]],
    gradeId: ['', [Validators.required]],
    thickness: [null as number | null, [this.positiveOptional]],
    width: [1250 as number | null, [this.positiveOptional]],
    weight: [null as number | null, [Validators.required, Validators.min(0.001)]],
    length: [0, [Validators.required, Validators.min(0)]],
    wattLossPerKg: [null as number | null, [this.positiveOptional]],
    warehouseLocation: ['', [Validators.maxLength(100)]],
    status: [CoilStatus.Available, [Validators.required]],
    receivedDate: [this.today, [Validators.required, this.notFutureDate]],
  });

  ngOnInit(): void {
    this.loadPageData();
  }

  protected submit(): void {
    this.apiErrors = [];
    this.form.markAllAsTouched();

    if (this.form.invalid) {
      this.apiErrors = ['Please fix the highlighted fields before updating the raw coil.'];
      this.focusFirstInvalidControl();
      return;
    }

    if (!this.rawCoil) {
      this.apiErrors = ['Raw coil details are still loading. Try again in a moment.'];
      return;
    }

    this.isSubmitting = true;
    this.rawCoilService
      .updateRawCoil(this.id, this.toRequest(this.rawCoil.rowVersion))
      .pipe(finalize(() => (this.isSubmitting = false)))
      .subscribe({
        next: (rawCoil) => {
          this.snackBar.open('Raw coil updated.', 'Close', { duration: 3000 });
          void this.router.navigate(['/raw-coils', rawCoil.id]);
        },
        error: (error: HttpErrorResponse) => this.captureError(error),
      });
  }

  protected errorFor(controlName: string): string {
    const control = this.form.get(controlName);
    if (!control || !control.touched || !control.errors) {
      return '';
    }

    if (control.hasError('required')) {
      return 'Required';
    }

    if (control.hasError('min') || control.hasError('positive')) {
      return 'Enter a valid value';
    }

    if (control.hasError('futureDate')) {
      return 'Date cannot be in the future';
    }

    return 'Invalid value';
  }

  private loadRawCoil(): void {
    this.loadPageData();
  }

  private loadPageData(): void {
    this.isLoading = true;
    forkJoin({
      rawCoil: this.rawCoilService.getRawCoilById(this.id),
      suppliers: this.lookupService.getSuppliers(),
      manufacturers: this.lookupService.getManufacturers(),
      grades: this.lookupService.getGrades(),
    })
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: ({ rawCoil, suppliers, manufacturers, grades }) => {
          this.rawCoil = rawCoil;
          this.suppliers = suppliers;
          this.manufacturers = manufacturers;
          this.grades = grades;
          this.form.patchValue({
            coilNumber: rawCoil.coilNumber,
            heatNumber: rawCoil.heatNumber,
            poNumber: rawCoil.poNumber ?? '',
            invoiceNo: rawCoil.invoiceNo ?? '',
            millTCNo: rawCoil.millTCNo ?? '',
            bisLicNumber: rawCoil.bisLicNumber ?? '',
            supplierId: rawCoil.supplierId,
            manufacturerId: rawCoil.manufacturerId,
            gradeId: rawCoil.gradeId,
            thickness: rawCoil.thickness,
            width: rawCoil.width,
            weight: rawCoil.weight,
            length: rawCoil.length,
            wattLossPerKg: rawCoil.wattLossPerKg,
            warehouseLocation: rawCoil.warehouseLocation ?? '',
            status: rawCoil.status,
            receivedDate: rawCoil.receivedDate.slice(0, 10),
          });
        },
        error: (error: HttpErrorResponse) => this.captureError(error),
      });
  }

  private toRequest(rowVersion: string): UpdateRawCoilRequest {
    const value = this.form.getRawValue();
    return {
      ...value,
      poNumber: value.poNumber || null,
      invoiceNo: value.invoiceNo || null,
      millTCNo: value.millTCNo || null,
      bisLicNumber: value.bisLicNumber || null,
      warehouseLocation: value.warehouseLocation || null,
      thickness: value.thickness ?? null,
      width: value.width ?? null,
      wattLossPerKg: value.wattLossPerKg ?? null,
      weight: value.weight ?? 0,
      length: value.length ?? 0,
      rowVersion,
    };
  }

  private captureError(error: HttpErrorResponse): void {
    if (error.status === 0) {
      this.apiErrors = ['The API is not reachable at http://localhost:5170. Start CoilManager.API and try again.'];
      return;
    }

    const body = error.error as { message?: string; errors?: string[] } | null;
    const message = body?.errors?.join('\n') || body?.message || error.message || 'Request failed.';
    this.apiErrors = error.status === 409 ? [`Conflict: ${message}`] : [message];
  }

  private focusFirstInvalidControl(): void {
    const invalidControl = this.elementRef.nativeElement.querySelector('[formControlName].ng-invalid') as HTMLElement | null;
    invalidControl?.focus();
  }

  private positiveOptional(control: AbstractControl<number | null>) {
    return control.value === null || control.value === undefined || control.value > 0 ? null : { positive: true };
  }

  private notFutureDate(control: AbstractControl<string>) {
    return control.value && control.value > new Date().toISOString().slice(0, 10) ? { futureDate: true } : null;
  }
}
