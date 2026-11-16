import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { RegisterComponent } from './register.component';

describe('RegisterComponent', () => {
  let authService: jasmine.SpyObj<AuthService>;
  let router: Router;

  beforeEach(async () => {
    authService = jasmine.createSpyObj<AuthService>('AuthService', ['register']);
    authService.register.and.returnValue(of({ message: 'Created' }));
    await TestBed.configureTestingModule({
      imports: [RegisterComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authService },
        { provide: ActivatedRoute, useValue: { snapshot: { queryParamMap: { has: () => false } } } }
      ]
    }).compileComponents();
    router = TestBed.inject(Router);
  });

  it('should register valid credentials and navigate to the login page', () => {
    const navigate = spyOn(router, 'navigate').and.returnValue(Promise.resolve(true));
    const fixture = TestBed.createComponent(RegisterComponent);
    const component = fixture.componentInstance;
    component.form.setValue({ email: 'person@example.com', password: 'secret1' });

    component.submit();

    expect(authService.register).toHaveBeenCalledWith({
      email: 'person@example.com',
      password: 'secret1'
    });
    expect(navigate).toHaveBeenCalledWith(['/login'], { queryParams: { registered: true } });
    expect(component.isSubmitting).toBeFalse();
  });

  it('should prevent registration and expose validation errors for missing or invalid fields', () => {
    const fixture = TestBed.createComponent(RegisterComponent);
    const component = fixture.componentInstance;
    component.submit();

    expect(authService.register).not.toHaveBeenCalled();
    expect(component.showError('email')).toBeTrue();
    expect(component.showError('password')).toBeTrue();

    component.form.setValue({ email: 'not-an-email', password: 'short' });
    component.submit();

    expect(authService.register).not.toHaveBeenCalled();
    expect(component.form.controls.email.invalid).toBeTrue();
    expect(component.form.controls.password.hasError('minlength')).toBeTrue();
  });

  it('should show a duplicate-email message when the API reports a conflict', () => {
    authService.register.and.returnValue(throwError(
      () => new HttpErrorResponse({ status: 409 })
    ));
    const fixture = TestBed.createComponent(RegisterComponent);
    const component = fixture.componentInstance;
    component.form.setValue({ email: 'person@example.com', password: 'secret1' });

    component.submit();

    expect(component.errorMessage).toBe('This email is already registered.');
    expect(component.isSubmitting).toBeFalse();
  });

  it('should display an API validation detail or a fallback for other failures', () => {
    const fixture = TestBed.createComponent(RegisterComponent);
    const component = fixture.componentInstance;
    component.form.setValue({ email: 'person@example.com', password: 'secret1' });
    authService.register.and.returnValue(throwError(
      () => new HttpErrorResponse({ status: 400, error: { detail: 'Email is invalid.' } })
    ));

    component.submit();
    expect(component.errorMessage).toBe('Email is invalid.');

    authService.register.and.returnValue(throwError(() => new Error('network')));
    component.submit();
    expect(component.errorMessage).toBe('Unable to create your account. Please try again.');
  });

  it('should ignore repeated submissions while registration is in progress', () => {
    const fixture = TestBed.createComponent(RegisterComponent);
    const component = fixture.componentInstance;
    component.form.setValue({ email: 'person@example.com', password: 'secret1' });
    component.isSubmitting = true;

    component.submit();

    expect(authService.register).not.toHaveBeenCalled();
  });
});
