import { Component, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html'
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
