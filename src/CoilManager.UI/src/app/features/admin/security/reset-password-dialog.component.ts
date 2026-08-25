import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { SecurityAdminService } from './security-admin.service';

@Component({
  selector: 'app-reset-password-dialog',
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatIconModule, MatInputModule],
  template: `
    <h2 mat-dialog-title><mat-icon>password</mat-icon>Reset password</h2>
    <mat-dialog-content>
      <p>Set a temporary password for <strong>{{ data.displayName }}</strong>. They will be required to change it at their next login.</p>
      <form id="reset-password-form" [formGroup]="form" (ngSubmit)="save()">
        <mat-form-field appearance="outline">
          <mat-label>Temporary password</mat-label>
          <input matInput [type]="showPassword() ? 'text' : 'password'" formControlName="temporaryPassword" autocomplete="new-password">
          <button mat-icon-button matSuffix type="button" (click)="showPassword.set(!showPassword())"><mat-icon>{{ showPassword() ? 'visibility_off' : 'visibility' }}</mat-icon></button>
          <mat-hint>12+ characters with upper/lower case, number and symbol.</mat-hint>
          @if (form.controls.temporaryPassword.touched && form.controls.temporaryPassword.invalid) { <mat-error>Enter a password of at least 12 characters.</mat-error> }
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Confirm temporary password</mat-label>
          <input matInput [type]="showPassword() ? 'text' : 'password'" formControlName="confirmPassword" autocomplete="new-password">
          @if (form.controls.confirmPassword.touched && form.controls.confirmPassword.invalid) { <mat-error>Confirm the temporary password.</mat-error> }
        </mat-form-field>
        @if (error()) { <p class="error" role="alert">{{ error() }}</p> }
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-stroked-button type="button" mat-dialog-close>Cancel</button>
      <button mat-flat-button type="submit" form="reset-password-form" [disabled]="saving()">{{ saving() ? 'Resetting...' : 'Reset password' }}</button>
    </mat-dialog-actions>
  `,
  styles: [`h2{display:flex!important;align-items:center;gap:10px}mat-dialog-content{width:min(520px,82vw)}form{display:grid;gap:8px;padding-top:8px}mat-form-field{width:100%}.error{margin:0;color:#ba1a1a}mat-dialog-actions{padding:16px 24px 22px}`],
})
export class ResetPasswordDialogComponent {
  private readonly api = inject(SecurityAdminService);
  private readonly dialogRef = inject(MatDialogRef<ResetPasswordDialogComponent>);
  protected readonly data = inject<{ userId: string; displayName: string }>(MAT_DIALOG_DATA);
  protected readonly saving = signal(false);
  protected readonly showPassword = signal(false);
  protected readonly error = signal('');
  protected readonly form = new FormGroup({
    temporaryPassword: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.minLength(12)] }),
    confirmPassword: new FormControl('', { nonNullable: true, validators: Validators.required }),
  });

  protected save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const { temporaryPassword, confirmPassword } = this.form.getRawValue();
    if (temporaryPassword !== confirmPassword) { this.error.set('Passwords do not match.'); return; }
    this.saving.set(true);
    this.error.set('');
    this.api.resetPassword(this.data.userId, temporaryPassword).subscribe({
      next: () => this.dialogRef.close(true),
      error: (error) => {
        this.error.set(error.error?.errors?.join(' ') || error.error?.message || 'Password could not be reset.');
        this.saving.set(false);
      },
    });
  }
}