import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const authService = inject(AuthService);
  if (!request.url.startsWith(`${environment.apiUrl}/api/`)) {
    return next(request);
  }

  authService.clearExpiredSession();
  const token = authService.accessToken;
  const authenticatedRequest = token
    ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : request;

  return next(authenticatedRequest).pipe(
    catchError((error: unknown) => {
      const isAuthRequest = request.url.includes('/api/auth/login')
        || request.url.includes('/api/auth/register');
      if (error instanceof HttpErrorResponse && error.status === 401 && !isAuthRequest) {
        authService.logout();
      }
      return throwError(() => error);
    })
  );
};
