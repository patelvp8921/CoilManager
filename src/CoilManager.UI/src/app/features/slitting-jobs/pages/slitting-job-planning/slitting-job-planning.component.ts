import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize, forkJoin, switchMap } from 'rxjs';
import { CoilStatus, statusLabel } from '../../../raw-coil/models/raw-coil.model';
import { VisualSlittingLayoutComponent } from '../../components/visual-slitting-layout/visual-slitting-layout.component';
import {
  CreateSlittingJobRequest,
  SlittingJob,
  SlittingJobStatus,
  SlittingMotherCoilLookup,
  UpdateSlittingJobRequest,
} from '../../models/slitting-job.model';
import { SlittingJobService } from '../../services/slitting-job.service';

interface SlitPreviewRow {
  sequenceNo: number;
  slitCoilId: string;
  width: number;
  estimatedWeight: number;
  remarks: string;
}

@Component({
  selector: 'app-slitting-job-planning',
  imports: [
    DecimalPipe,
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatChipsModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressBarModule,
    MatSelectModule,
    MatSnackBarModule,
    VisualSlittingLayoutComponent,
  ],
  templateUrl: './slitting-job-planning.component.html',
  styleUrl: './slitting-job-planning.component.scss',
})
export class SlittingJobPlanningComponent implements OnInit {
  private readonly maxSlits = 10;
  private readonly crgoDensityKgPerCubicMeter = 7650;
  private readonly cubicMillimetersPerCubicMeter = 1_000_000_000;
  private readonly meterLikeLengthThreshold = 10_000;
  private readonly millimetersPerMeter = 1_000;
  private readonly fb = inject(FormBuilder);
  private readonly slittingJobService = inject(SlittingJobService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly today = new Date().toISOString().slice(0, 10);
  protected readonly nextJobNumber = signal('');
  protected readonly editJobId = signal<string | null>(null);
  private readonly rowVersion = signal('');
  protected readonly motherCoils = signal<readonly SlittingMotherCoilLookup[]>([]);
  protected readonly selectedMotherCoil = signal<SlittingMotherCoilLookup | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly isSubmitting = signal(false);
  protected readonly apiErrors = signal<readonly string[]>([]);
  protected readonly motherCoilSearch = signal('');
  private readonly formRevision = signal(0);
  protected readonly availableMotherCoils = computed(() =>
    this.motherCoils().filter((coil) => coil.status === CoilStatus.Available));
  protected readonly filteredMotherCoils = computed(() => {
    const search = this.motherCoilSearch().trim().toLowerCase();
    const selectedMotherCoil = this.selectedMotherCoil();
    const filteredCoils = search
      ? this.availableMotherCoils().filter((coil) => this.matchesMotherCoilSearch(coil, search))
      : this.availableMotherCoils();

    if (!selectedMotherCoil || filteredCoils.some((coil) => coil.id === selectedMotherCoil.id)) {
      return filteredCoils;
    }

    return [selectedMotherCoil, ...filteredCoils];
  });
  protected readonly isEditMode = computed(() => this.editJobId() !== null);
  protected readonly pageTitle = computed(() => this.isEditMode() ? 'Edit Slitting Job' : 'Create Slitting Job');
  protected readonly displayedJobNumber = computed(() => this.nextJobNumber() || (this.isEditMode() ? 'Loading...' : 'Loading...'));
  protected readonly canSubmit = computed(() => !this.isLoading() && !this.isSubmitting() && !this.form.disabled);

  protected readonly form = this.fb.nonNullable.group({
    planningDate: [this.today, [Validators.required]],
    plannerId: [''],
    motherCoilId: ['', [Validators.required]],
    machineId: [''],
    shift: [''],
    numberOfSlits: [0, [Validators.required, Validators.min(1), Validators.max(this.maxSlits)]],
    knifeThickness: [0.2, [Validators.required, Validators.min(0)]],
    leftEdgeTrim: [5, [Validators.required, Validators.min(0)]],
    rightEdgeTrim: [5, [Validators.required, Validators.min(0)]],
    remarks: [''],
    items: this.fb.array([]),
  });

  protected readonly slitRows = computed<readonly SlitPreviewRow[]>(() => {
    this.formRevision();
    const motherCoil = this.selectedMotherCoil();
    return this.items.controls.map((control, index) => {
      const value = control.getRawValue();
      const sequenceNo = index + 1;
      const width = this.parseNumber(value.width);
      return {
        sequenceNo,
        slitCoilId: motherCoil ? this.generateSlitCoilId(this.displayedJobNumber(), sequenceNo) : '',
        width,
        estimatedWeight: this.estimateWeight(width),
        remarks: value.remarks ?? '',
      };
    });
  });

  protected readonly summary = computed(() => {
    this.formRevision();
    const motherCoil = this.selectedMotherCoil();
    const motherWidth = motherCoil?.width ?? 0;
    const totalSlitWidth = this.slitRows().reduce((total, row) => total + row.width, 0);
    const numberOfCuts = Math.max(this.slitRows().length - 1, 0);
    const knifeLoss = numberOfCuts * this.parseNumber(this.form.controls.knifeThickness.value);
    const edgeTrim = this.parseNumber(this.form.controls.leftEdgeTrim.value) + this.parseNumber(this.form.controls.rightEdgeTrim.value);
    const totalPlannedWidth = totalSlitWidth + knifeLoss + edgeTrim;
    const remainingWidth = Math.max(motherWidth - totalPlannedWidth, 0);
    const excessWidth = Math.max(totalPlannedWidth - motherWidth, 0);
    const utilizationPercent = motherWidth > 0 ? totalSlitWidth / motherWidth * 100 : 0;

    return {
      motherWidth,
      totalSlitWidth,
      totalPlannedWidth,
      numberOfCuts,
      knifeLoss,
      edgeTrim,
      remainingWidth,
      excessWidth,
      utilizationPercent,
      isOverAllocated: excessWidth > 0,
    };
  });

  protected readonly weightSummary = computed(() => {
    this.formRevision();
    const motherCoil = this.selectedMotherCoil();
    const motherWeight = motherCoil?.weight ?? 0;
    const totalSlitWeight = this.slitRows().reduce((total, row) => total + row.estimatedWeight, 0);
    const trimScrapWeight = this.estimateWeight(this.summary().edgeTrim);
    const knifeLossWeight = this.estimateWeight(this.summary().knifeLoss);
    const processScrapWeight = trimScrapWeight + knifeLossWeight;
    const balanceWeight = motherWeight > 0
      ? Math.max(motherWeight - totalSlitWeight - processScrapWeight, 0)
      : this.estimateWeight(this.summary().remainingWidth);
    const yieldPercent = motherWeight > 0
      ? totalSlitWeight / motherWeight * 100
      : this.summary().utilizationPercent;

    return {
      motherWeight,
      totalSlitWeight,
      trimScrapWeight,
      knifeLossWeight,
      processScrapWeight,
      balanceWeight,
      yieldPercent,
    };
  });

  get items(): FormArray {
    return this.form.controls.items;
  }

  ngOnInit(): void {
    const jobId = this.route.snapshot.paramMap.get('id');
    this.editJobId.set(jobId);
    this.isLoading.set(true);

    if (jobId) {
      forkJoin({
        job: this.slittingJobService.getSlittingJobById(jobId),
        motherCoils: this.slittingJobService.searchMotherCoils(''),
      })
        .pipe(finalize(() => this.isLoading.set(false)))
        .subscribe({
          next: ({ job, motherCoils }) => this.loadExistingJob(job, motherCoils),
          error: (error: unknown) => this.captureError(error),
        });
    } else {
      forkJoin({
        nextJobNumber: this.slittingJobService.getNextJobNumber(),
        motherCoils: this.slittingJobService.searchMotherCoils(''),
      })
        .pipe(finalize(() => this.isLoading.set(false)))
        .subscribe({
          next: ({ nextJobNumber, motherCoils }) => {
            this.nextJobNumber.set(nextJobNumber);
            this.motherCoils.set(this.filterAvailableMotherCoils(motherCoils));
          },
          error: (error: unknown) => this.captureError(error),
        });
    }

    this.form.valueChanges.subscribe(() => {
      this.apiErrors.set([]);
      this.bumpFormRevision();
    });

    this.form.controls.numberOfSlits.valueChanges.subscribe((value) => {
      this.resizeSlitRows(Number(value || 0), true);
    });
  }

  protected searchMotherCoils(): void {
    this.slittingJobService.searchMotherCoils('').subscribe({
      next: (motherCoils) => this.motherCoils.set(this.withSelectedMotherCoil(this.filterAvailableMotherCoils(motherCoils))),
      error: (error: unknown) => this.captureError(error),
    });
  }

  protected updateMotherCoilSearch(event: Event): void {
    this.motherCoilSearch.set((event.target as HTMLInputElement).value);
  }

  protected selectMotherCoil(motherCoilId: string): void {
    const motherCoil = this.motherCoils().find((item) => item.id === motherCoilId) ?? null;
    this.selectedMotherCoil.set(motherCoil);
    this.items.clear();
    this.resizeSlitRows(Number(this.form.controls.numberOfSlits.value || 0), false);
    this.bumpFormRevision();
  }

  protected generateSlits(): void {
    const numberOfSlits = Number(this.form.controls.numberOfSlits.value || 0);
    this.resizeSlitRows(numberOfSlits, true);
  }

  protected saveDraft(): void {
    this.submit(false);
  }

  protected releaseJob(): void {
    this.submit(true);
  }

  protected labelForCoilStatus(status: CoilStatus): string {
    return statusLabel(status);
  }

  private submit(release: boolean): void {
    this.apiErrors.set([]);
    this.form.markAllAsTouched();
    this.form.updateValueAndValidity({ emitEvent: false });
    this.items.updateValueAndValidity({ emitEvent: false });

    const validationErrors = this.collectSubmitValidationErrors();
    if (validationErrors.length) {
      this.apiErrors.set(validationErrors);
      return;
    }

    if (this.summary().isOverAllocated) {
      this.apiErrors.set(['Allocated width must not exceed mother coil width.']);
      return;
    }

    this.isSubmitting.set(true);
    const saveRequest = this.isEditMode()
      ? this.slittingJobService
          .getSlittingJobById(this.editJobId()!)
          .pipe(switchMap((job) => {
            this.rowVersion.set(job.rowVersion);
            return this.slittingJobService.updateSlittingJob(this.editJobId()!, this.toUpdateRequest());
          }))
      : this.slittingJobService.createSlittingJob(this.toRequest());

    saveRequest
      .subscribe({
        next: (job) => {
          this.rowVersion.set(job.rowVersion);
          if (!release) {
            this.isSubmitting.set(false);
            this.snackBar.open(this.isEditMode() ? 'Slitting job draft updated.' : 'Slitting job draft saved.', 'Close', { duration: 3000 });
            void this.router.navigate(['/slitting-jobs']);
            return;
          }

          this.slittingJobService
            .releaseSlittingJob(job.id)
            .pipe(finalize(() => this.isSubmitting.set(false)))
            .subscribe({
              next: () => {
                this.snackBar.open('Slitting job released.', 'Close', { duration: 3000 });
                void this.router.navigate(['/slitting-jobs']);
              },
              error: (error: unknown) => this.captureError(error),
            });
        },
        error: (error: unknown) => {
          this.isSubmitting.set(false);
          this.captureError(error);
        },
      });
  }

  private toRequest(): CreateSlittingJobRequest {
    const value = this.form.getRawValue();
    return {
      planningDate: value.planningDate,
      plannerId: value.plannerId || null,
      motherCoilId: value.motherCoilId,
      machineId: value.machineId || null,
      shift: value.shift || null,
      knifeThickness: Number(value.knifeThickness),
      leftEdgeTrim: Number(value.leftEdgeTrim),
      rightEdgeTrim: Number(value.rightEdgeTrim),
      remarks: value.remarks || null,
      items: this.slitRows().map((row) => ({
        sequenceNo: row.sequenceNo,
        width: row.width,
        remarks: row.remarks || null,
      })),
    };
  }

  private toUpdateRequest(): UpdateSlittingJobRequest {
    return {
      ...this.toRequest(),
      rowVersion: this.rowVersion(),
    };
  }

  private loadExistingJob(job: SlittingJob, motherCoils: readonly SlittingMotherCoilLookup[]): void {
    this.nextJobNumber.set(job.slittingJobNo);
    this.rowVersion.set(job.rowVersion);
    this.motherCoils.set(this.withExistingMotherCoil(motherCoils, job));
    this.selectedMotherCoil.set(this.toMotherCoilLookup(job));
    this.items.clear();

    this.form.patchValue({
      planningDate: job.planningDate,
      plannerId: job.plannerId ?? '',
      motherCoilId: job.motherCoilId,
      machineId: job.machineId ?? '',
      shift: job.shift ?? '',
      numberOfSlits: job.items.length,
      knifeThickness: job.knifeThickness,
      leftEdgeTrim: job.leftEdgeTrim,
      rightEdgeTrim: job.rightEdgeTrim,
      remarks: job.remarks ?? '',
    }, { emitEvent: false });

    for (const item of [...job.items].sort((left, right) => left.sequenceNo - right.sequenceNo)) {
      this.items.push(this.createSlitItemGroup(item.width, item.remarks ?? ''));
    }

    if (job.status !== SlittingJobStatus.Draft) {
      this.apiErrors.set(['Only draft slitting jobs can be edited.']);
      this.form.disable();
    }

    this.bumpFormRevision();
  }

  private resetAfterSave(): void {
    this.form.reset({
      planningDate: this.today,
      plannerId: '',
      motherCoilId: '',
      machineId: '',
      shift: '',
      numberOfSlits: 0,
      knifeThickness: 0.2,
      leftEdgeTrim: 5,
      rightEdgeTrim: 5,
      remarks: '',
      items: [],
    });
    this.items.clear();
    this.selectedMotherCoil.set(null);
    this.bumpFormRevision();
    this.slittingJobService.getNextJobNumber().subscribe((jobNumber) => this.nextJobNumber.set(jobNumber));
  }

  private bumpFormRevision(): void {
    this.formRevision.update((revision) => revision + 1);
  }

  private resizeSlitRows(targetCount: number, preserveExisting: boolean): void {
    if (targetCount < 0 || targetCount > this.maxSlits) {
      return;
    }

    const existingValues = preserveExisting
      ? this.items.controls.map((control) => control.getRawValue())
      : [];

    while (this.items.length > targetCount) {
      this.items.removeAt(this.items.length - 1);
    }

    while (this.items.length < targetCount) {
      const existingValue = existingValues[this.items.length];
      this.items.push(this.createSlitItemGroup(
        Number(existingValue?.width ?? 0),
        existingValue?.remarks ?? ''));
    }

    this.bumpFormRevision();
  }

  private createSlitItemGroup(width = 0, remarks = '') {
    return this.fb.nonNullable.group({
      width: [width, [Validators.required, Validators.min(0.001)]],
      remarks: [remarks],
    });
  }

  private filterAvailableMotherCoils(motherCoils: readonly SlittingMotherCoilLookup[]): readonly SlittingMotherCoilLookup[] {
    return motherCoils.filter((coil) => coil.status === CoilStatus.Available);
  }

  private matchesMotherCoilSearch(coil: SlittingMotherCoilLookup, search: string): boolean {
    return [
      coil.motherCoilId,
      coil.coilNumber,
      coil.heatNumber,
      coil.supplierName,
      coil.manufacturerName,
      coil.grade,
    ].some((value) => value?.toLowerCase().includes(search));
  }

  private withExistingMotherCoil(motherCoils: readonly SlittingMotherCoilLookup[], job: SlittingJob): readonly SlittingMotherCoilLookup[] {
    const filteredMotherCoils = this.filterAvailableMotherCoils(motherCoils);
    if (filteredMotherCoils.some((coil) => coil.id === job.motherCoilId)) {
      return filteredMotherCoils;
    }

    return [this.toMotherCoilLookup(job), ...filteredMotherCoils];
  }

  private withSelectedMotherCoil(motherCoils: readonly SlittingMotherCoilLookup[]): readonly SlittingMotherCoilLookup[] {
    const selectedMotherCoil = this.selectedMotherCoil();
    if (!selectedMotherCoil || motherCoils.some((coil) => coil.id === selectedMotherCoil.id)) {
      return motherCoils;
    }

    return [selectedMotherCoil, ...motherCoils];
  }

  private toMotherCoilLookup(job: SlittingJob): SlittingMotherCoilLookup {
    return {
      id: job.motherCoilId,
      motherCoilId: job.motherCoilNo,
      coilNumber: job.supplierCoilNumber ?? '',
      heatNumber: job.heatNumber ?? '',
      supplierName: job.supplierName,
      manufacturerName: job.manufacturerName,
      grade: job.grade,
      thickness: job.thickness,
      category: job.category,
      coreLossPerKg: job.coreLossPerKg,
      width: job.motherCoilWidth,
      weight: job.motherCoilWeight,
      length: job.motherCoilLength,
      warehouseLocation: job.warehouseLocation,
      status: job.motherCoilStatus,
    };
  }

  private estimateWeight(width: number): number {
    const motherCoil = this.selectedMotherCoil();
    if (!motherCoil || width <= 0) {
      return 0;
    }

    if (motherCoil.width > 0 && motherCoil.weight > 0) {
      const proportionalWeight = motherCoil.weight * width / motherCoil.width;
      return Math.round(proportionalWeight * 1000) / 1000;
    }

    if (motherCoil.thickness <= 0 || motherCoil.length <= 0) {
      return 0;
    }

    const lengthInMillimeters = this.normalizeLengthToMillimeters(motherCoil.length);
    const weight = width * motherCoil.thickness * lengthInMillimeters * this.crgoDensityKgPerCubicMeter / this.cubicMillimetersPerCubicMeter;
    return Math.round(weight * 1000) / 1000;
  }

  private normalizeLengthToMillimeters(length: number): number {
    return length < this.meterLikeLengthThreshold
      ? length * this.millimetersPerMeter
      : length;
  }

  private generateSlitCoilId(slittingJobNo: string, sequenceNo: number): string {
    const sequence = sequenceNo.toString().padStart(2, '0');
    const normalizedJobNo = slittingJobNo.replace(/\//g, '-');
    return normalizedJobNo.startsWith('AE-S-')
      ? `SC-${normalizedJobNo.slice(5)}-${sequence}`
      : `SC-${normalizedJobNo}-${sequence}`;
  }

  private collectSubmitValidationErrors(): readonly string[] {
    const errors: string[] = [];

    if (!this.form.controls.planningDate.value) {
      errors.push('Planning date is required.');
    }

    if (!this.form.controls.motherCoilId.value || !this.selectedMotherCoil()) {
      errors.push('Select a Mother Coil.');
    }

    if (this.items.length === 0) {
      errors.push('Generate at least one slit row.');
    }

    const invalidSlitRows = this.getInvalidSlitRowNumbers();
    if (invalidSlitRows.length) {
      errors.push(`Enter width greater than 0 for slit row ${invalidSlitRows.join(', ')}.`);
    }

    if (this.form.controls.numberOfSlits.invalid) {
      errors.push(`Number of slits must be between 1 and ${this.maxSlits}.`);
    }

    if (this.form.controls.knifeThickness.invalid || this.form.controls.leftEdgeTrim.invalid || this.form.controls.rightEdgeTrim.invalid) {
      errors.push('Knife thickness and edge trim values must be 0 or greater.');
    }

    if (this.isEditMode() && !this.rowVersion()) {
      errors.push('Draft version is still loading. Please try again in a moment.');
    }

    return errors;
  }

  private getInvalidSlitRowNumbers(): readonly number[] {
    return this.items.controls
      .map((control, index) => ({
        sequenceNo: index + 1,
        width: this.parseNumber(control.get('width')?.value),
      }))
      .filter((row) => !Number.isFinite(row.width) || row.width <= 0)
      .map((row) => row.sequenceNo);
  }

  private parseNumber(value: unknown): number {
    if (typeof value === 'number') {
      return value;
    }

    if (typeof value === 'string') {
      return Number(value.trim().replace(/,/g, ''));
    }

    return Number(value ?? 0);
  }

  private captureError(error: unknown): void {
    if (!(error instanceof HttpErrorResponse)) {
      this.apiErrors.set([error instanceof Error ? error.message : 'Request failed.']);
      return;
    }

    if (error.status === 0) {
      this.apiErrors.set(['The API is not reachable at http://localhost:5170. Start CoilManager.API and try again.']);
      return;
    }

    const body = error.error as { message?: string; errors?: string[] } | null;
    this.apiErrors.set(body?.errors?.length ? body.errors : [body?.message || error.message || 'Request failed.']);
  }
}
