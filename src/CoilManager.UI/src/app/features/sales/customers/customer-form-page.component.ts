import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SalesService } from '../sales.service';

@Component({
  selector: 'app-customer-form-page',
  imports: [ReactiveFormsModule, RouterLink, MatButtonModule, MatCardModule, MatFormFieldModule,
    MatIconModule, MatInputModule, MatProgressBarModule, MatSlideToggleModule],
  templateUrl: './customer-form-page.component.html',
  styleUrl: './customer-form-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CustomerFormPageComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly service = inject(SalesService);
  private readonly snackBar = inject(MatSnackBar);
  protected readonly id = this.route.snapshot.paramMap.get('id');
  protected readonly viewOnly = signal(!!this.id && !this.router.url.endsWith('/edit'));
  protected readonly isLoading = signal(false);
  protected readonly isSubmitting = signal(false);
  protected readonly form = this.fb.group({
    customerCode: [{ value: 'Loading...', disabled: true }],
    customerName: ['', [Validators.required, Validators.maxLength(200)]],
    shortName: ['', Validators.maxLength(80)],
    billingAddress: ['', [Validators.required, Validators.maxLength(1000)]],
    shippingAddress: ['', Validators.maxLength(1000)],
    city: ['', [Validators.required, Validators.maxLength(100)]],
    state: ['', [Validators.required, Validators.maxLength(100)]],
    country: ['India', [Validators.required, Validators.maxLength(100)]],
    postalCode: ['', [Validators.required, Validators.maxLength(20)]],
    contactPerson: ['', [Validators.required, Validators.maxLength(160)]],
    phone: ['', [Validators.required, Validators.maxLength(40)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(254)]],
    gstNumber: ['', Validators.maxLength(30)],
    pan: ['', Validators.maxLength(20)],
    paymentTerms: ['', Validators.maxLength(250)],
    creditDays: [null as number | null, Validators.min(0)],
    isActive: [true],
    remarks: ['', Validators.maxLength(2000)],
    rowVersion: [''],
  });

  ngOnInit(): void {
    this.isLoading.set(true);
    if (this.id) {
      this.service.customer(this.id).subscribe({
        next: customer => { this.form.patchValue(customer); if (this.viewOnly()) this.form.disable(); this.isLoading.set(false); },
        error: () => this.isLoading.set(false),
      });
    } else {
      this.service.nextCustomerCode().subscribe({
        next: code => { this.form.controls.customerCode.setValue(code); this.isLoading.set(false); },
        error: () => {
          this.form.controls.customerCode.setValue('Generated when saved');
          this.snackBar.open('The customer-code preview could not be loaded. The code will still be generated when saved.', 'Close', { duration: 5000 });
          this.isLoading.set(false);
        },
      });
    }
  }

  protected save(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.viewOnly()) return;
    this.isSubmitting.set(true);
    this.service.saveCustomer(this.form.getRawValue() as never, this.id ?? undefined).subscribe({
      next: customer => {
        this.snackBar.open('Customer saved successfully.', 'Close', { duration: 2500 });
        this.router.navigate(['/customers']);
      },
      error: () => this.isSubmitting.set(false),
    });
  }
}
