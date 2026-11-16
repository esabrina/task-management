import { HttpClient, HttpErrorResponse, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AuthService } from '../services/auth.service';
import { environment } from '../../../environments/environment';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  let httpTestingController: HttpTestingController;
  let logout: jasmine.Spy;
  let clearExpiredSession: jasmine.Spy;
  let token: string | null;

  beforeEach(() => {
    token = 'access-token';
    logout = jasmine.createSpy('logout');
    clearExpiredSession = jasmine.createSpy('clearExpiredSession');
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        {
          provide: AuthService,
          useValue: {
            clearExpiredSession,
            logout,
            get accessToken() {
              return token;
            }
          }
        }
      ]
    });
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTestingController.verify());

  it('should attach the bearer token to API requests', () => {
    TestBed.inject(HttpClient).get(`${environment.apiUrl}/api/tasks`).subscribe();
    const request = httpTestingController.expectOne(`${environment.apiUrl}/api/tasks`);

    expect(clearExpiredSession).toHaveBeenCalled();
    expect(request.request.headers.get('Authorization')).toBe('Bearer access-token');
    request.flush([]);
  });

  it('should leave non-API requests unchanged', () => {
    TestBed.inject(HttpClient).get('https://example.com/data').subscribe();
    const request = httpTestingController.expectOne('https://example.com/data');

    expect(clearExpiredSession).not.toHaveBeenCalled();
    expect(request.request.headers.has('Authorization')).toBeFalse();
    request.flush({});
  });

  it('should redirect to login when a protected API request returns unauthorized', () => {
    let error: unknown;
    TestBed.inject(HttpClient)
      .get(`${environment.apiUrl}/api/tasks`)
      .subscribe({ error: (value: unknown) => error = value });
    httpTestingController.expectOne(`${environment.apiUrl}/api/tasks`)
      .flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(logout).toHaveBeenCalled();
    expect(error).toEqual(jasmine.any(HttpErrorResponse));
  });

  it('should preserve unauthorized login responses without logging the user out', () => {
    let error: unknown;
    TestBed.inject(HttpClient)
      .post(`${environment.apiUrl}/api/auth/login`, {})
      .subscribe({ error: (value: unknown) => error = value });
    httpTestingController.expectOne(`${environment.apiUrl}/api/auth/login`)
      .flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(logout).not.toHaveBeenCalled();
    expect(error).toEqual(jasmine.any(HttpErrorResponse));
  });
});
