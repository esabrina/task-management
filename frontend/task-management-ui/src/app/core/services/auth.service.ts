import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse } from '../models/auth-response.model';
import { User } from '../models/user.model';

interface Credentials extends User {
  password: string;
}

const SESSION_KEY = 'task-management-session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly session = signal<AuthResponse | null>(this.readSession());
  private expirationTimer: ReturnType<typeof setTimeout> | null = null;

  constructor(
    private readonly http: HttpClient,
    private readonly router: Router
  ) {
    this.scheduleExpiration();
  }

  isAuthenticated(): boolean {
    const session = this.session();
    return !!session?.accessToken && Date.parse(session.expiresAt) > Date.now();
  }

  get accessToken(): string | null {
    return this.isAuthenticated() ? this.session()?.accessToken ?? null : null;
  }

  register(credentials: Credentials): Observable<{ message: string | null }> {
    return this.http.post<{ message: string | null }>(
      `${environment.apiUrl}/api/auth/register`,
      credentials
    );
  }

  login(credentials: Credentials): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(
      `${environment.apiUrl}/api/auth/login`,
      credentials
    ).pipe(tap((response) => this.storeSession(response)));
  }

  logout(): void {
    this.clearSession();
    void this.router.navigate(['/login']);
  }

  clearExpiredSession(): void {
    if (this.session() && !this.isAuthenticated()) {
      this.clearSession();
    }
  }

  private storeSession(response: AuthResponse): void {
    if (!response.accessToken || !Number.isFinite(Date.parse(response.expiresAt))) {
      throw new Error('The API returned an invalid authentication response.');
    }
    localStorage.setItem(SESSION_KEY, JSON.stringify(response));
    this.session.set(response);
    this.scheduleExpiration();
  }

  private clearSession(): void {
    if (this.expirationTimer !== null) {
      clearTimeout(this.expirationTimer);
      this.expirationTimer = null;
    }
    this.session.set(null);
    localStorage.removeItem(SESSION_KEY);
  }

  private scheduleExpiration(): void {
    if (this.expirationTimer !== null) {
      clearTimeout(this.expirationTimer);
    }

    const expiresAt = this.session()?.expiresAt;
    if (!expiresAt) {
      this.expirationTimer = null;
      return;
    }

    this.expirationTimer = setTimeout(
      () => this.logout(),
      Math.max(0, Date.parse(expiresAt) - Date.now())
    );
  }

  private readSession(): AuthResponse | null {
    const stored = localStorage.getItem(SESSION_KEY);
    if (!stored) {
      return null;
    }

    try {
      const session = JSON.parse(stored) as AuthResponse;
      if (session.accessToken && Number.isFinite(Date.parse(session.expiresAt))) {
        return session;
      }
    } catch (error: unknown) {
      if (!(error instanceof SyntaxError)) {
        throw error;
      }
    }

    localStorage.removeItem(SESSION_KEY);
    return null;
  }
}
