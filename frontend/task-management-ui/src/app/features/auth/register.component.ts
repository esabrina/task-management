import { Component, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <main class="app-shell d-flex align-items-center justify-content-center py-5">
      <section class="card auth-card">
        <div class="card-body p-4 p-md-5">
          <div class="mb-4">
            <p class="text-primary fw-semibold mb-2">TASK MANAGEMENT</p>
            <h1 class="h3 mb-2">Create your account</h1>
            <p class="text-secondary mb-0">Get started organizing your work.</p>
          </div>

          @if (errorMessage) {
            <div class="alert alert-danger" role="alert">{{ errorMessage }}</div>
          }

          <form [formGroup]="form" (ngSubmit)="submit()" novalidate>
            <div class="mb-3">
              <label for="email" class="form-label">Email</label>
              <input id="email" type="email" class="form-control"
                formControlName="email" autocomplete="email"
                [class.is-invalid]="showError('email')">
              @if (showError('email')) {
                <div class="invalid-feedback">
                  {{ form.controls.email.hasError('required') ? 'Email is required.' : 'Enter a valid email address.' }}
                </div>
              }
            </div>
            <div class="mb-4">
              <label for="password" class="form-label">Password</label>
              <input id="password" type="password" class="form-control"
                formControlName="password" autocomplete="new-password"
                [class.is-invalid]="showError('password')">
              @if (showError('password')) {
                <div class="invalid-feedback">
                  {{ form.controls.password.hasError('required') ? 'Password is required.' : 'Password must be at least 6 characters.' }}
                </div>
              }
            </div>
            <button type="submit" class="btn btn-primary w-100" [disabled]="isSubmitting">
              {{ isSubmitting ? 'Creating account…' : 'Create account' }}
            </button>
          </form>

          <p class="text-center text-secondary mt-4 mb-0">
            Already registered? <a routerLink="/login" class="link-primary">Sign in</a>
          </p>
        </div>
      </section>
    </main>
  `
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
