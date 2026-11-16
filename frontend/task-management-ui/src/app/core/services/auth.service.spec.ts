import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { Router } from '@angular/router';
import { AuthResponse } from '../models/auth-response.model';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';

describe('AuthService', () => {
  let service: AuthService;
  let httpTestingController: HttpTestingController;
  let navigate: jasmine.Spy;

  beforeEach(() => {
    localStorage.clear();
    navigate = jasmine.createSpy('navigate').and.returnValue(Promise.resolve(true));
    TestBed.configureTestingModule({
      providers: [
        AuthService,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: Router, useValue: { navigate } }
      ]
    });
    service = TestBed.inject(AuthService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    service.logout();
    httpTestingController.verify();
    localStorage.clear();
  });

  it('should register an account without creating an authenticated session', () => {
    let response: { message: string | null } | undefined;
    service.register({ email: 'person@example.com', password: 'secret1' }).subscribe((value) => {
      response = value;
    });

    const request = httpTestingController.expectOne(`${environment.apiUrl}/api/auth/register`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ email: 'person@example.com', password: 'secret1' });
    request.flush({ message: 'Created' });

    expect(response).toEqual({ message: 'Created' });
    expect(service.isAuthenticated()).toBeFalse();
  });

  it('should store a valid login response and expose its token', () => {
    const session: AuthResponse = {
      accessToken: 'access-token',
      expiresAt: new Date(Date.now() + 60_000).toISOString()
    };
    service.login({ email: 'person@example.com', password: 'secret1' }).subscribe();
    httpTestingController.expectOne(`${environment.apiUrl}/api/auth/login`).flush(session);

    expect(service.isAuthenticated()).toBeTrue();
    expect(service.accessToken).toBe('access-token');
    expect(JSON.parse(localStorage.getItem('task-management-session') ?? '{}')).toEqual(session);
  });

  it('should reject a malformed authentication response without storing it', () => {
    let error: unknown;
    service.login({ email: 'person@example.com', password: 'secret1' }).subscribe({
      error: (value: unknown) => error = value
    });
    httpTestingController.expectOne(`${environment.apiUrl}/api/auth/login`).flush({
      accessToken: '',
      expiresAt: 'not-a-date'
    });

    expect(error).toEqual(jasmine.any(Error));
    expect(localStorage.getItem('task-management-session')).toBeNull();
    expect(service.isAuthenticated()).toBeFalse();
  });

  it('should clear an expired session and redirect to login when the token expires', fakeAsync(() => {
    service.login({ email: 'person@example.com', password: 'secret1' }).subscribe();
    httpTestingController.expectOne(`${environment.apiUrl}/api/auth/login`).flush({
      accessToken: 'short-lived-token',
      expiresAt: new Date(Date.now() + 1000).toISOString()
    });

    tick(1000);

    expect(service.isAuthenticated()).toBeFalse();
    expect(localStorage.getItem('task-management-session')).toBeNull();
    expect(navigate).toHaveBeenCalledWith(['/login']);
  }));

  it('should discard an expired session restored from storage', () => {
    localStorage.setItem('task-management-session', JSON.stringify({
      accessToken: 'expired-token',
      expiresAt: new Date(Date.now() - 1000).toISOString()
    }));
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        AuthService,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: Router, useValue: { navigate } }
      ]
    });
    service = TestBed.inject(AuthService);

    expect(service.isAuthenticated()).toBeFalse();
    service.clearExpiredSession();
    expect(localStorage.getItem('task-management-session')).toBeNull();
  });

  it('should discard malformed stored session data', () => {
    localStorage.setItem('task-management-session', '{invalid json');
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        AuthService,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: Router, useValue: { navigate } }
      ]
    });
    service = TestBed.inject(AuthService);

    expect(service.isAuthenticated()).toBeFalse();
    expect(localStorage.getItem('task-management-session')).toBeNull();
  });
});
