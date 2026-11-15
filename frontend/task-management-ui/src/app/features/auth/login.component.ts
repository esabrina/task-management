import { Component, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <main class="app-shell d-flex align-items-center justify-content-center py-5">
      <section class="card auth-card">
        <div class="card-body p-4 p-md-5">
          <div class="mb-4">
            <p class="text-primary fw-semibold mb-2">TASK MANAGEMENT</p>
            <h1 class="h3 mb-2">Welcome back</h1>
            <p class="text-secondary mb-0">Sign in to continue to your tasks.</p>
          </div>

          @if (errorMessage) {
            <div class="alert alert-danger" role="alert">{{ errorMessage }}</div>
          }
          @if (registeredMessage) {
            <div class="alert alert-success" role="status">{{ registeredMessage }}</div>
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
                formControlName="password" autocomplete="current-password"
                [class.is-invalid]="showError('password')">
              @if (showError('password')) {
                <div class="invalid-feedback">Password is required.</div>
              }
            </div>
            <button type="submit" class="btn btn-primary w-100" [disabled]="isSubmitting">
              {{ isSubmitting ? 'Signing in…' : 'Sign in' }}
            </button>
          </form>

          <p class="text-center text-secondary mt-4 mb-0">
            Don't have an account? <a routerLink="/register" class="link-primary">Create one</a>
          </p>
        </div>
      </section>
    </main>
  `
})
export class LoginComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly form = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    password: ['', Validators.required]
  });
  isSubmitting = false;
  errorMessage = '';
  readonly registeredMessage = this.route.snapshot.queryParamMap.has('registered')
    ? 'Account created successfully. You can now sign in.'
    : '';

  showError(controlName: 'email' | 'password'): boolean {
    const control = this.form.controls[controlName];
    return control.invalid && (control.touched || this.formSubmitted);
  }

  private formSubmitted = false;

  submit(): void {
    this.formSubmitted = true;
    this.errorMessage = '';
    if (this.form.invalid || this.isSubmitting) {
      this.form.markAllAsTouched();
      if (!this.form.controls.email.value || !this.form.controls.password.value) {
        this.errorMessage = 'Invalid email or password';
      }
      return;
    }

    this.isSubmitting = true;
    this.authService.login(this.form.getRawValue()).subscribe({
      next: () => {
        this.isSubmitting = false;
        void this.router.navigate(['/tasks']);
      },
      error: (error: unknown) => {
        this.isSubmitting = false;
        this.errorMessage = error instanceof HttpErrorResponse && error.status === 401
          ? 'Invalid email or password'
          : 'Unable to sign in. Please try again.';
      }
    });
  }
}
