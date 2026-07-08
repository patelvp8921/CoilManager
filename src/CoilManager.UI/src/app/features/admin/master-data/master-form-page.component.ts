import { HttpErrorResponse } from '@angular/common/http';
import { DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize, timeout } from 'rxjs';
import { MasterDataService } from './master-data.service';
import { MasterRecord, MasterRouteData } from './master-data.model';

@Component({
  selector: 'app-master-form-page',
  imports: [
    ReactiveFormsModule,
    DecimalPipe,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressBarModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatSnackBarModule,
  ],
  templateUrl: './master-form-page.component.html',
  styleUrl: './master-form-page.component.scss',
})
export class MasterFormPageComponent implements OnInit {
  protected readonly route = inject(ActivatedRoute);
  protected readonly router = inject(Router);
  protected readonly routeData = this.route.snapshot.data as MasterRouteData;
  protected readonly recordId = this.route.snapshot.paramMap.get('id');
  protected readonly isManufacturer = this.routeData.type === 'manufacturers';
  protected readonly isSupplier = this.routeData.type === 'suppliers';
  protected readonly isGrade = this.routeData.type === 'grades';
  protected readonly thicknessOptions = [
    { thicknessMm: 0.23, category: 'M3' },
    { thicknessMm: 0.27, category: 'M4' },
    { thicknessMm: 0.30, category: 'M5' },
    { thicknessMm: 0.35, category: 'M6' },
  ];
  protected readonly isEditMode = computed(() => !!this.recordId);
  protected readonly isLoading = signal(false);
  protected readonly isSubmitting = signal(false);
  protected readonly apiErrors = signal<readonly string[]>([]);
  protected readonly record = signal<MasterRecord | null>(null);

  private readonly formBuilder = inject(FormBuilder);
  private readonly service = inject(MasterDataService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly form = this.formBuilder.nonNullable.group({
    code: ['', [Validators.required, Validators.maxLength(50)]],
    name: ['', [Validators.required, Validators.maxLength(150)]],
    country: ['', [Validators.maxLength(100)]],
    address: ['', [Validators.maxLength(300)]],
    gst: ['', [Validators.maxLength(30)]],
    email: ['', [Validators.email, Validators.maxLength(150)]],
    contactNo: ['', [Validators.maxLength(30)]],
    description: ['', [Validators.maxLength(250)]],
    thicknessMm: [null as number | null],
    category: [''],
    coreLossPerKg: [null as number | null],
    isActive: [true],
  });

  ngOnInit(): void {
    if (this.isSupplier) {
      this.form.controls.code.clearValidators();
      this.form.controls.code.updateValueAndValidity();
    }

    if (this.isGrade) {
      this.form.controls.name.clearValidators();
      this.form.controls.name.updateValueAndValidity();
      this.form.controls.description.clearValidators();
      this.form.controls.description.updateValueAndValidity();
      this.form.controls.thicknessMm.setValidators([Validators.required]);
      this.form.controls.thicknessMm.updateValueAndValidity();
      this.form.controls.coreLossPerKg.setValidators([Validators.required, Validators.min(0.0001)]);
      this.form.controls.coreLossPerKg.updateValueAndValidity();
      this.form.controls.thicknessMm.valueChanges.subscribe((thicknessMm) => {
        this.form.controls.category.setValue(this.categoryForThickness(thicknessMm), { emitEvent: false });
      });
    }

    if (this.recordId) {
      this.loadRecord(this.recordId);
    }
  }

  protected submit(): void {
    this.apiErrors.set([]);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const request = {
      code: this.isSupplier ? null : value.code.trim(),
      name: this.isGrade ? value.code.trim() : value.name.trim(),
      description: this.isGrade ? null : value.description.trim() || null,
      country: this.isManufacturer ? value.country.trim() || null : null,
      address: this.isSupplier ? value.address.trim() || null : null,
      gst: this.isSupplier ? value.gst.trim() || null : null,
      email: this.isSupplier ? value.email.trim() || null : null,
      contactNo: this.isSupplier ? value.contactNo.trim() || null : null,
      grade: this.isGrade ? value.code.trim() : null,
      thicknessMm: this.isGrade ? value.thicknessMm : null,
      coreLossPerKg: this.isGrade ? value.coreLossPerKg : null,
      isActive: value.isActive,
      rowVersion: this.record()?.rowVersion ?? null,
    };

    this.isSubmitting.set(true);
    const save$ = this.recordId
      ? this.service.update(this.routeData.type, this.recordId, request)
      : this.service.create(this.routeData.type, request);

    save$
      .pipe(
        timeout(15000),
        finalize(() => this.isSubmitting.set(false)),
      )
      .subscribe({
        next: () => {
          this.snackBar.open(`${this.routeData.singular} saved successfully.`, 'Close', { duration: 3000 });
          this.router.navigate(['/admin', this.routeData.type]);
        },
        error: (error: HttpErrorResponse) => this.showError(error),
      });
  }

  private loadRecord(id: string): void {
    this.isLoading.set(true);
    this.service
      .getById(this.routeData.type, id)
      .pipe(
        timeout(15000),
        finalize(() => this.isLoading.set(false)),
      )
      .subscribe({
        next: (record) => {
          this.record.set(record);
          this.form.patchValue({
            code: record.code,
            name: record.name,
            country: record.country ?? '',
            address: record.address ?? '',
            gst: record.gst ?? '',
            email: record.email ?? '',
            contactNo: record.contactNo ?? '',
            description: record.description ?? '',
            thicknessMm: record.thicknessMm ?? null,
            category: record.category ?? this.categoryForThickness(record.thicknessMm ?? null),
            coreLossPerKg: record.coreLossPerKg ?? null,
            isActive: record.isActive,
          });
        },
        error: (error: HttpErrorResponse) => this.showError(error),
      });
  }

  private showError(error: HttpErrorResponse): void {
    const body = error.error as { message?: string; errors?: string[] } | null;
    const errors = body?.errors?.length ? body.errors : [body?.message || error.message || 'Request failed.'];
    this.apiErrors.set(errors);
    this.snackBar.open(errors.join('\n'), 'Close', { duration: 6000 });
  }

  private categoryForThickness(thicknessMm: number | null): string {
    return this.thicknessOptions.find((option) => option.thicknessMm === thicknessMm)?.category ?? '';
  }
}
