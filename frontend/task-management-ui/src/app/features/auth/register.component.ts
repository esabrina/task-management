import { Component, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html'
})
export class RegisterComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly form = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });
  isSubmitting = false;
  errorMessage = '';
  private formSubmitted = false;

  showError(controlName: 'email' | 'password'): boolean {
    const control = this.form.controls[controlName];
    return control.invalid && (control.touched || this.formSubmitted);
  }

  submit(): void {
    this.formSubmitted = true;
    this.errorMessage = '';
    if (this.form.invalid || this.isSubmitting) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.authService.register(this.form.getRawValue()).subscribe({
      next: () => {
        this.isSubmitting = false;
        void this.router.navigate(['/login'], { queryParams: { registered: true } });
      },
      error: (error: unknown) => {
        this.isSubmitting = false;
        this.errorMessage = error instanceof HttpErrorResponse && error.status === 409
          ? 'This email is already registered.'
          : this.apiErrorMessage(error) ?? 'Unable to create your account. Please try again.';
      }
    });
  }

  private apiErrorMessage(error: unknown): string | null {
    if (!(error instanceof HttpErrorResponse)) {
      return null;
    }
    const body: unknown = error.error;
    if (typeof body === 'object' && body !== null && 'detail' in body) {
      const detail = body.detail;
      return typeof detail === 'string' && detail ? detail : null;
    }
    return null;
  }
}
