import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { finalize, forkJoin, startWith } from 'rxjs';
import { LookupItem } from '../../shared/models/lookup-item.model';
import { LookupService } from '../../shared/services/lookup.service';
import {
  operationLabels,
  productLabels,
  typeLabels,
  WorkOrder,
  WorkOrderProductType,
  WorkOrderRequest,
  WorkOrderType,
} from './work-order.model';
import { WorkOrderService } from './work-order.service';

@Component({
  selector: 'app-work-order-form',
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
  ],
  templateUrl: './work-order-form.component.html',
  styleUrl: './work-order-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WorkOrderFormComponent implements OnInit {
  protected readonly typeLabels = typeLabels;
  protected readonly productLabels = productLabels;
  protected readonly grades = signal<readonly LookupItem[]>([]);
  protected readonly number = signal('Loading...');
  protected readonly isLoading = signal(true);
  protected readonly isSaving = signal(false);
  protected readonly apiErrors = signal<readonly string[]>([]);
  protected readonly editId = signal<string | null>(null);

  private rowVersion = '';
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(WorkOrderService);
  private readonly lookup = inject(LookupService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly form = this.fb.group({
    workOrderType: [0, Validators.required],
    productType: [1, Validators.required],
    customerName: [''],
    salesOrderReference: [''],
    workOrderDate: [new Date().toISOString().slice(0, 10), Validators.required],
    requiredDate: [''],
    priority: [3, Validators.required],
    gradeId: [null as string | null],
    thickness: [0.23, [Validators.required, Validators.min(0.001)]],
    category: ['CRGO', Validators.required],
    coreLossPerKg: [0, Validators.min(0)],
    drawingNumber: [''],
    requiredWidth: [null as number | null, Validators.min(0.001)],
    requiredWeight: [null as number | null, Validators.min(0.001)],
    requiredQuantity: [null as number | null, Validators.min(1)],
    remarks: [''],
  });

  private readonly formValue = toSignal(
    this.form.valueChanges.pipe(startWith(this.form.getRawValue())),
    { initialValue: this.form.getRawValue() },
  );

  protected readonly routing = computed(() => {
    const product = this.formValue().productType ?? 1;
    return operationLabels.map((name, index) => ({
      name,
      required: index === 2
        || (index === 0 && (product === 1 || product === 2))
        || (index === 1 && product === 2),
    }));
  });

  protected readonly summary = computed(() => {
    const value = this.formValue();
    return {
      source: typeLabels[value.workOrderType ?? 0],
      product: productLabels[value.productType ?? 1],
      requiredWeight: value.requiredWeight ?? 0,
      requiredQuantity: value.requiredQuantity ?? 0,
      priority: value.priority ?? 3,
      grade: this.grades().find((grade) => grade.id === value.gradeId)?.code ?? 'Not specified',
    };
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    this.editId.set(id);

    if (id) {
      forkJoin({ grades: this.lookup.getGrades(), workOrder: this.service.get(id) })
        .pipe(finalize(() => this.isLoading.set(false)))
        .subscribe({
          next: ({ grades, workOrder }) => {
            this.grades.set(grades);
            this.loadWorkOrder(workOrder);
          },
          error: (error: unknown) => this.showError(error),
        });
      return;
    }

    forkJoin({ grades: this.lookup.getGrades(), number: this.service.nextNumber() })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: ({ grades, number }) => {
          this.grades.set(grades);
          this.number.set(number);
        },
        error: (error: unknown) => this.showError(error),
      });
  }

  protected gradeChanged(id: string | null): void {
    const grade = this.grades().find((item) => item.id === id);
    if (!grade) return;
    this.form.patchValue({
      thickness: grade.thicknessMm ?? this.form.controls.thickness.value,
      category: grade.category ?? this.form.controls.category.value,
      coreLossPerKg: grade.coreLossPerKg ?? this.form.controls.coreLossPerKg.value,
    });
  }

  protected save(): void {
    this.apiErrors.set([]);
    this.form.markAllAsTouched();
    if (this.form.invalid || this.isSaving()) return;

    this.isSaving.set(true);
    const value = this.form.getRawValue();
    const request: WorkOrderRequest = {
      workOrderType: value.workOrderType as WorkOrderType,
      productType: value.productType as WorkOrderProductType,
      customerName: value.customerName,
      salesOrderReference: value.salesOrderReference,
      workOrderDate: value.workOrderDate!,
      requiredDate: value.requiredDate || null,
      priority: value.priority!,
      gradeId: value.gradeId,
      thickness: value.thickness!,
      category: value.category!,
      coreLossPerKg: value.coreLossPerKg!,
      drawingNumber: value.drawingNumber,
      requiredWidth: value.requiredWidth,
      requiredWeight: value.requiredWeight,
      requiredQuantity: value.requiredQuantity,
      remarks: value.remarks,
      rowVersion: this.rowVersion,
    };
    const id = this.editId();
    const saveRequest = id ? this.service.update(id, request) : this.service.create(request);

    saveRequest
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: (workOrder) => {
          this.snackBar.open('Work Order saved successfully.', 'Close', { duration: 3000 });
          void this.router.navigate(['/work-orders', workOrder.id]);
        },
        error: (error: unknown) => this.showError(error),
      });
  }

  protected errorFor(controlName: keyof typeof this.form.controls): string {
    const control = this.form.controls[controlName];
    if (!control.touched || !control.errors) return '';
    if (control.errors['required']) return 'This field is required.';
    if (control.errors['min']) return `Minimum value is ${control.errors['min'].min}.`;
    return 'Invalid value.';
  }

  private loadWorkOrder(workOrder: WorkOrder): void {
    this.number.set(workOrder.workOrderNumber);
    this.rowVersion = workOrder.rowVersion;
    this.form.patchValue({
      workOrderType: workOrder.workOrderType,
      productType: workOrder.productType,
      customerName: workOrder.customerName ?? '',
      salesOrderReference: workOrder.salesOrderReference ?? '',
      workOrderDate: workOrder.workOrderDate,
      requiredDate: workOrder.requiredDate ?? '',
      priority: workOrder.priority,
      gradeId: workOrder.gradeId ?? null,
      thickness: workOrder.thickness,
      category: workOrder.category,
      coreLossPerKg: workOrder.coreLossPerKg,
      drawingNumber: workOrder.drawingNumber ?? '',
      requiredWidth: workOrder.requiredWidth ?? null,
      requiredWeight: workOrder.requiredWeight ?? null,
      requiredQuantity: workOrder.requiredQuantity ?? null,
      remarks: workOrder.remarks ?? '',
    });
  }

  private showError(error: unknown): void {
    const response = error as { error?: { message?: string; errors?: string[] }; message?: string };
    const errors = response.error?.errors?.length
      ? response.error.errors
      : [response.error?.message ?? response.message ?? 'Unable to load the Work Order form.'];
    this.apiErrors.set(errors);
  }
}
