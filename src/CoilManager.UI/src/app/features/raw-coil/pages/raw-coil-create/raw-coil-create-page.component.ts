import { HttpErrorResponse } from '@angular/common/http';
import { Component, ElementRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AbstractControl, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { Router, RouterLink } from '@angular/router';
import QRCode from 'qrcode';
import { finalize, forkJoin } from 'rxjs';
import { LookupItem } from '../../../../shared/models/lookup-item.model';
import { LookupService } from '../../../../shared/services/lookup.service';
import { CoilPreviewComponent, CoilPreviewModel } from '../../components/coil-preview/coil-preview.component';
import { COIL_STATUS_OPTIONS, CoilStatus, CreateRawCoilRequest } from '../../models/raw-coil.model';
import { RawCoilService } from '../../services/raw-coil.service';

@Component({
  selector: 'app-raw-coil-create-page',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatDividerModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressBarModule,
    MatSelectModule,
    MatSnackBarModule,
    MatTabsModule,
    CoilPreviewComponent,
  ],
  templateUrl: './raw-coil-create-page.component.html',
  styleUrl: './raw-coil-create-page.component.scss',
})
export class RawCoilCreatePageComponent {
  protected readonly statusOptions = COIL_STATUS_OPTIONS;
  protected readonly today = new Date().toISOString().slice(0, 10);
  protected isSubmitting = false;
  protected isLoadingLookups = false;
  protected apiErrors: readonly string[] = [];
  protected nextCoilId = '';
  protected suppliers: readonly LookupItem[] = [];
  protected manufacturers: readonly LookupItem[] = [];
  protected grades: readonly LookupItem[] = [];
  protected readonly preview = signal<CoilPreviewModel>(this.createEmptyPreview());
  protected readonly qrCodeDataUrl = signal('');
  protected readonly isQrGenerating = signal(false);

  private readonly fb = inject(FormBuilder);
  private readonly elementRef = inject(ElementRef<HTMLElement>);
  private readonly lookupService = inject(LookupService);
  private readonly rawCoilService = inject(RawCoilService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

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
    this.form.valueChanges.pipe(takeUntilDestroyed()).subscribe(() => {
      this.qrCodeDataUrl.set('');
      this.updatePreview();
    });
    this.form.controls.gradeId.valueChanges.pipe(takeUntilDestroyed()).subscribe((gradeId) => this.populateGradeDetails(gradeId));
    this.loadNextCoilId();
    this.loadLookups();
  }

  protected submit(): void {
    this.apiErrors = [];
    this.form.markAllAsTouched();

    if (this.form.invalid) {
      this.apiErrors = ['Please fix the highlighted fields before creating the mother coil.'];
      this.focusFirstInvalidControl();
      return;
    }

    this.isSubmitting = true;
    this.rawCoilService
      .createRawCoil(this.toRequest())
      .pipe(finalize(() => (this.isSubmitting = false)))
      .subscribe({
        next: (rawCoil) => {
          this.snackBar.open('Mother coil created.', 'Close', { duration: 3000 });
          this.openCoilDetails(rawCoil.id);
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

  protected clearForm(): void {
    this.apiErrors = [];
    this.qrCodeDataUrl.set('');
    this.form.reset({
      coilNumber: '',
      heatNumber: '',
      poNumber: '',
      invoiceNo: '',
      millTCNo: '',
      bisLicNumber: '',
      supplierId: '',
      manufacturerId: '',
      gradeId: '',
      thickness: null,
      category: '',
      coreLossPerKg: null,
      width: 1250,
      weight: null,
      length: 0,
      warehouseLocation: '',
      status: CoilStatus.Available,
      receivedDate: this.today,
    });
    this.loadNextCoilId();
    this.updatePreview();
  }

  protected saveAsDraft(): void {
    this.apiErrors = [];

    if (!this.validateDraftIdentifiers()) {
      this.apiErrors = ['Coil Number, Supplier, Mill, and Grade are required before saving a draft.'];
      this.focusFirstInvalidControl();
      return;
    }

    this.isSubmitting = true;
    this.rawCoilService
      .createRawCoil(this.toRequest(CoilStatus.Draft))
      .pipe(finalize(() => (this.isSubmitting = false)))
      .subscribe({
        next: (rawCoil) => {
          this.snackBar.open('Draft saved successfully', 'Close', { duration: 3000 });
          this.openCoilDetails(rawCoil.id);
        },
        error: (error: HttpErrorResponse) => this.captureError(error),
      });
  }

  protected async generateQrCode(): Promise<void> {
    this.isQrGenerating.set(true);
    const current = this.preview();
    const payload = {
      MotherCoilId: current.coilId,
      Supplier: current.supplier,
      Manufacturer: current.manufacturer,
      Grade: current.grade,
      Thickness: current.thickness,
      Category: current.category,
      CoreLossPerKg: current.coreLossPerKg,
      Width: current.width,
      Weight: current.weight,
    };

    try {
      this.qrCodeDataUrl.set(await QRCode.toDataURL(JSON.stringify(payload), {
        errorCorrectionLevel: 'M',
        margin: 2,
        scale: 6,
        type: 'image/png',
      }));
    } catch {
      this.snackBar.open('Unable to generate QR code.', 'Close', { duration: 3000 });
    } finally {
      this.isQrGenerating.set(false);
    }
  }

  private toRequest(status = this.form.controls.status.value): CreateRawCoilRequest {
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
      status,
    };
  }

  private captureError(error: HttpErrorResponse): void {
    if (error.status === 0) {
      this.apiErrors = ['The API is not reachable at http://localhost:5170. Start CoilManager.API and try again.'];
      return;
    }

    const body = error.error as { message?: string; errors?: string[] } | null;
    this.apiErrors = body?.errors?.length ? body.errors : [body?.message || error.message || 'Request failed.'];
  }

  private openCoilDetails(rawCoilId: string): void {
    if (!rawCoilId) {
      this.apiErrors = ['Mother coil was saved, but the API did not return the new mother coil id.'];
      return;
    }

    void this.router.navigateByUrl(`/mother-coils/${rawCoilId}/details`, { replaceUrl: true });
  }

  private loadLookups(): void {
    this.isLoadingLookups = true;
    forkJoin({
      suppliers: this.lookupService.getSuppliers(),
      manufacturers: this.lookupService.getManufacturers(),
      grades: this.lookupService.getGrades(),
    })
      .pipe(finalize(() => (this.isLoadingLookups = false)))
      .subscribe({
        next: ({ suppliers, manufacturers, grades }) => {
          this.suppliers = suppliers;
          this.manufacturers = manufacturers;
          this.grades = grades;
          this.populateGradeDetails(this.form.controls.gradeId.value);
          this.updatePreview();
        },
        error: (error: HttpErrorResponse) => this.captureError(error),
      });
  }

  private loadNextCoilId(): void {
    this.rawCoilService.getNextCoilId().subscribe({
      next: (nextCoilId) => {
        this.nextCoilId = nextCoilId;
        this.updatePreview();
      },
      error: (error: HttpErrorResponse) => this.captureError(error),
    });
  }

  private updatePreview(): void {
    const value = this.form.getRawValue();
    const selectedGrade = this.selectedGrade(value.gradeId);
    this.preview.set({
      coilId: this.nextCoilId,
      supplier: this.lookupNameOnly(this.suppliers, value.supplierId),
      manufacturer: this.lookupNameOnly(this.manufacturers, value.manufacturerId),
      grade: this.lookupCodeOnly(this.grades, value.gradeId),
      thickness: selectedGrade?.thicknessMm ?? value.thickness,
      category: selectedGrade?.category ?? value.category,
      coreLossPerKg: selectedGrade?.coreLossPerKg ?? value.coreLossPerKg,
      width: value.width,
      weight: value.weight,
      status: value.status,
    });
  }

  private createEmptyPreview(): CoilPreviewModel {
    return {
      coilId: '',
      supplier: '',
      manufacturer: '',
      grade: '',
      thickness: null,
      category: '',
      coreLossPerKg: null,
      width: 1250,
      weight: null,
      status: CoilStatus.Available,
    };
  }

  private lookupName(items: readonly LookupItem[], id: string): string {
    const match = items.find((item) => item.id === id);
    if (!match) {
      return '';
    }

    return match.code ? `${match.name} (${match.code})` : match.name;
  }

  private lookupNameOnly(items: readonly LookupItem[], id: string): string {
    return items.find((item) => item.id === id)?.name ?? '';
  }

  private lookupCodeOnly(items: readonly LookupItem[], id: string): string {
    const match = items.find((item) => item.id === id);
    return match?.code || match?.name || '';
  }

  private selectedGrade(id: string): LookupItem | undefined {
    return this.grades.find((grade) => grade.id === id);
  }

  private populateGradeDetails(gradeId: string): void {
    const grade = this.selectedGrade(gradeId);
    this.form.patchValue(
      {
        thickness: grade?.thicknessMm ?? null,
        category: grade?.category ?? '',
        coreLossPerKg: grade?.coreLossPerKg ?? null,
      },
      { emitEvent: false },
    );
    this.updatePreview();
  }

  private validateDraftIdentifiers(): boolean {
    const controls = [
      this.form.controls.coilNumber,
      this.form.controls.supplierId,
      this.form.controls.manufacturerId,
      this.form.controls.gradeId,
    ];

    controls.forEach((control) => control.markAsTouched());
    return controls.every((control) => control.valid);
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
