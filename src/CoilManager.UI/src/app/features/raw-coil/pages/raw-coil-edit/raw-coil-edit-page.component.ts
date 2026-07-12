import { HttpErrorResponse } from '@angular/common/http';
import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, ElementRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
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
import { finalize, forkJoin, timeout } from 'rxjs';
import { LookupItem } from '../../../../shared/models/lookup-item.model';
import { LookupService } from '../../../../shared/services/lookup.service';
import { COIL_STATUS_OPTIONS, CoilStatus, RawCoil, UpdateRawCoilRequest } from '../../models/raw-coil.model';
import { RawCoilService } from '../../services/raw-coil.service';
import { SlitCoil } from '../../../slit-coils/models/slit-coil.model';
import { SlitCoilService } from '../../../slit-coils/services/slit-coil.service';

@Component({
  selector: 'app-raw-coil-edit-page',
  imports: [
    ReactiveFormsModule,
    DatePipe,
    DecimalPipe,
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
  protected readonly isLoading = signal(false);
  protected readonly isSubmitting = signal(false);
  protected readonly apiErrors = signal<readonly string[]>([]);
  protected readonly rawCoil = signal<RawCoil | null>(null);
  protected readonly suppliers = signal<readonly LookupItem[]>([]);
  protected readonly manufacturers = signal<readonly LookupItem[]>([]);
  protected readonly grades = signal<readonly LookupItem[]>([]);
  protected readonly generatedSlitCoils = signal<readonly SlitCoil[]>([]);

  private readonly fb = inject(FormBuilder);
  private readonly elementRef = inject(ElementRef<HTMLElement>);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly lookupService = inject(LookupService);
  private readonly rawCoilService = inject(RawCoilService);
  private readonly slitCoilService = inject(SlitCoilService);
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
    thickness: [null as number | null],
    category: [''],
    coreLossPerKg: [null as number | null],
    width: [1250 as number | null, [this.positiveOptional]],
    weight: [null as number | null, [Validators.required, Validators.min(0.001)]],
    length: [0, [Validators.required, Validators.min(0)]],
    warehouseLocation: ['', [Validators.maxLength(100)]],
    status: [CoilStatus.Available, [Validators.required]],
    receivedDate: [this.today, [Validators.required, this.notFutureDate]],
  });

  constructor() {
    this.form.controls.gradeId.valueChanges.pipe(takeUntilDestroyed()).subscribe((gradeId) => this.populateGradeDetails(gradeId));
  }

  ngOnInit(): void {
    this.loadPageData();
  }

  protected submit(): void {
    this.apiErrors.set([]);
    if (this.isFrozen()) {
      this.apiErrors.set(['Consumed Mother Coil details are frozen and cannot be edited.']);
      return;
    }
    this.form.markAllAsTouched();

    if (this.form.invalid) {
      this.apiErrors.set(['Please fix the highlighted fields before updating the mother coil.']);
      this.focusFirstInvalidControl();
      return;
    }

    const rawCoil = this.rawCoil();
    if (!rawCoil) {
      this.apiErrors.set(['Mother coil details are still loading. Try again in a moment.']);
      return;
    }

    this.isSubmitting.set(true);
    this.rawCoilService
      .updateRawCoil(this.id, this.toRequest(rawCoil.rowVersion))
      .pipe(
        timeout(15000),
        finalize(() => this.isSubmitting.set(false)),
      )
      .subscribe({
        next: () => {
          this.snackBar.open('Mother coil saved.', 'Close', { duration: 3000 });
          void this.router.navigate(['/mother-coils']);
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

  private loadPageData(): void {
    this.isLoading.set(true);
    this.apiErrors.set([]);
    forkJoin({
      rawCoil: this.rawCoilService.getRawCoilById(this.id),
      suppliers: this.lookupService.getSuppliers(),
      manufacturers: this.lookupService.getManufacturers(),
      grades: this.lookupService.getGrades(),
    })
      .pipe(
        timeout(15000),
        finalize(() => this.isLoading.set(false)),
      )
      .subscribe({
        next: ({ rawCoil, suppliers, manufacturers, grades }) => {
          this.rawCoil.set(rawCoil);
          this.slitCoilService.getSlitCoils({ page: 1, pageSize: 100, motherCoilNumber: rawCoil.rawCoilNumber }).subscribe(response => this.generatedSlitCoils.set(response.data));
          this.suppliers.set(this.withCurrentLookup(suppliers, rawCoil.supplierId, rawCoil.supplierName));
          this.manufacturers.set(this.withCurrentLookup(manufacturers, rawCoil.manufacturerId, rawCoil.manufacturerName));
          this.grades.set(this.withCurrentGradeLookup(grades, rawCoil));
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
            category: rawCoil.category,
            coreLossPerKg: rawCoil.coreLossPerKg,
            width: rawCoil.width,
            weight: rawCoil.weight,
            length: rawCoil.length,
            warehouseLocation: rawCoil.warehouseLocation ?? '',
            status: rawCoil.status,
            receivedDate: rawCoil.receivedDate.slice(0, 10),
          });
          if (rawCoil.status === CoilStatus.Consumed) {
            this.form.disable({ emitEvent: false });
          }
        },
        error: (error: HttpErrorResponse) => this.captureError(error),
      });
  }

  protected isFrozen(): boolean {
    return this.rawCoil()?.status === CoilStatus.Consumed;
  }

  private toRequest(rowVersion: string): UpdateRawCoilRequest {
    const value = this.form.getRawValue();
    return {
      coilNumber: value.coilNumber,
      heatNumber: value.heatNumber,
      supplierId: value.supplierId,
      manufacturerId: value.manufacturerId,
      gradeId: value.gradeId,
      poNumber: value.poNumber || null,
      invoiceNo: value.invoiceNo || null,
      millTCNo: value.millTCNo || null,
      bisLicNumber: value.bisLicNumber || null,
      warehouseLocation: value.warehouseLocation || null,
      width: value.width ?? null,
      weight: value.weight ?? 0,
      length: value.length ?? 0,
      receivedDate: value.receivedDate,
      status: value.status,
      rowVersion,
    };
  }

  private captureError(error: HttpErrorResponse): void {
    if (error.status === 0) {
      this.apiErrors.set(['The API is not reachable at http://localhost:5170. Start CoilManager.API and try again.']);
      return;
    }

    const body = error.error as { message?: string; errors?: string[] } | null;
    const message = body?.errors?.join('\n') || body?.message || error.message || 'Request failed.';
    this.apiErrors.set(error.status === 409 ? [`Conflict: ${message}`] : [message]);
  }

  private withCurrentLookup(items: readonly LookupItem[], id: string, name: string): readonly LookupItem[] {
    if (!id || items.some((item) => item.id === id)) {
      return items;
    }

    return [{ id, name, code: 'Inactive' }, ...items];
  }

  private withCurrentGradeLookup(items: readonly LookupItem[], rawCoil: RawCoil): readonly LookupItem[] {
    if (!rawCoil.gradeId || items.some((item) => item.id === rawCoil.gradeId)) {
      return items;
    }

    return [
      {
        id: rawCoil.gradeId,
        name: rawCoil.grade,
        code: rawCoil.grade,
        thicknessMm: rawCoil.thickness,
        category: rawCoil.category,
        coreLossPerKg: rawCoil.coreLossPerKg,
      },
      ...items,
    ];
  }

  private populateGradeDetails(gradeId: string): void {
    const grade = this.grades().find((item) => item.id === gradeId);
    this.form.patchValue(
      {
        thickness: grade?.thicknessMm ?? null,
        category: grade?.category ?? '',
        coreLossPerKg: grade?.coreLossPerKg ?? null,
      },
      { emitEvent: false },
    );
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
