import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../core/services/auth.service';
import { LoginComponent } from './login.component';

describe('LoginComponent', () => {
  let authService: jasmine.SpyObj<AuthService>;
  let router: Router;
  let registered: boolean;

  beforeEach(async () => {
    registered = false;
    authService = jasmine.createSpyObj<AuthService>('AuthService', ['login']);
    authService.login.and.returnValue(of({
      accessToken: 'access-token',
      expiresAt: new Date(Date.now() + 60_000).toISOString()
    }));
    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authService },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { queryParamMap: { has: (name: string) => name === 'registered' && registered } } }
        }
      ]
    }).compileComponents();
    router = TestBed.inject(Router);
  });

  it('should show the registration confirmation when redirected from registration', () => {
    registered = true;
    const fixture = TestBed.createComponent(LoginComponent);

    expect(fixture.componentInstance.registeredMessage)
      .toBe('Account created successfully. You can now sign in.');
  });

  it('should authenticate valid credentials and navigate to the task list', () => {
    const navigate = spyOn(router, 'navigate').and.returnValue(Promise.resolve(true));
    const fixture = TestBed.createComponent(LoginComponent);
    const component = fixture.componentInstance;
    component.form.setValue({ email: 'person@example.com', password: 'secret1' });

    component.submit();

    expect(authService.login).toHaveBeenCalledWith({
      email: 'person@example.com',
      password: 'secret1'
    });
    expect(navigate).toHaveBeenCalledWith(['/tasks']);
    expect(component.isSubmitting).toBeFalse();
  });

  it('should prevent login and display field errors when credentials are missing or invalid', () => {
    const fixture = TestBed.createComponent(LoginComponent);
    const component = fixture.componentInstance;
    component.submit();

    expect(authService.login).not.toHaveBeenCalled();
    expect(component.errorMessage).toBe('Invalid email or password');
    expect(component.showError('email')).toBeTrue();
    expect(component.showError('password')).toBeTrue();

    component.form.setValue({ email: 'invalid-email', password: 'secret1' });
    component.submit();
    expect(authService.login).not.toHaveBeenCalled();
    expect(component.form.controls.email.invalid).toBeTrue();
  });

  it('should show the invalid-credentials message for an unauthorized login', () => {
    authService.login.and.returnValue(throwError(
      () => new HttpErrorResponse({ status: 401 })
    ));
    const fixture = TestBed.createComponent(LoginComponent);
    const component = fixture.componentInstance;
    component.form.setValue({ email: 'person@example.com', password: 'wrong-password' });

    component.submit();

    expect(component.errorMessage).toBe('Invalid email or password');
    expect(component.isSubmitting).toBeFalse();
  });

  it('should show a fallback message for unexpected login failures', () => {
    authService.login.and.returnValue(throwError(() => new Error('network')));
    const fixture = TestBed.createComponent(LoginComponent);
    const component = fixture.componentInstance;
    component.form.setValue({ email: 'person@example.com', password: 'secret1' });

    component.submit();

    expect(component.errorMessage).toBe('Unable to sign in. Please try again.');
    expect(component.isSubmitting).toBeFalse();
  });
});
