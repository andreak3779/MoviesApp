import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap, throwError } from 'rxjs';
import { API_BASE_URL } from '../api-config';

/**
 * Shape of the JSON returned by {@code POST /api/v1/Auth/login} and
 * {@code POST /api/v1/Auth/refresh}. Matches the API's {@code AuthResponseDto}.
 */
export interface AuthResponse {
  token: string;
  refreshToken: string;
  expiresAt: string;
  email: string;
  firstName?: string | null;
  lastName?: string | null;
}

export interface LoginPayload {
  email: string;
  password: string;
}

export interface RegisterPayload {
  email: string;
  password: string;
  confirmPassword: string;
  firstName?: string;
  lastName?: string;
}

export interface RefreshPayload {
  token: string;
  refreshToken: string;
}

const TOKEN_KEY = 'moviesapp.accessToken';
const REFRESH_KEY = 'moviesapp.refreshToken';

/**
 * Stores the access token in {@code sessionStorage} and exposes a signal
 * for the rest of the app. The refresh token is kept in the same place for
 * now — moving it to a SameSite=Strict cookie is a separate, larger change.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  private readonly _token = signal<string | null>(sessionStorage.getItem(TOKEN_KEY));
  private readonly _refresh = signal<string | null>(sessionStorage.getItem(REFRESH_KEY));

  /** Current access token, or null when not signed in. */
  readonly token = this._token.asReadonly();
  /** Whether a session is currently established. */
  readonly isAuthenticated = computed(() => this._token() !== null);

  login(payload: LoginPayload): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/Auth/login`, payload)
      .pipe(tap((r) => this.persist(r)));
  }

  register(payload: RegisterPayload): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/Auth/register`, payload)
      .pipe(tap((r) => this.persist(r)));
  }

  refresh(payload: RefreshPayload): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/Auth/refresh`, payload)
      .pipe(tap((r) => this.persist(r)));
  }

  /**
   * Exchanges the stored refresh token for a new session.
   *
   * The error interceptor calls this when the API rejects a request with a
   * 401: the access token has most likely expired, and the refresh token is
   * still good for exactly one exchange.
   */
  refreshSession(): Observable<AuthResponse> {
    const token = this._token();
    const refreshToken = this._refresh();

    if (!token || !refreshToken) {
      return throwError(() => new Error('There is no session to refresh.'));
    }

    return this.refresh({ token, refreshToken });
  }

  logout(): Observable<void> {
    const token = this._token();
    const refreshToken = this._refresh();
    const headers = token ? { Authorization: `Bearer ${token}` } : undefined;

    // The API's logout takes the same pair as the refresh endpoint: both
    // fields are required, and the access token identifies the caller whose
    // refresh token is being revoked.
    return this.http
      .post<void>(`${this.baseUrl}/Auth/logout`, { token, refreshToken }, { headers })
      .pipe(tap({ next: () => this.clear(), error: () => this.clear() }));
  }

  /** Synchronous read used by the auth interceptor. */
  getRefreshToken(): string | null {
    return this._refresh();
  }

  /**
   * Ends the session locally, without calling the API.
   *
   * The error interceptor uses this when a request fails with a 401 that a
   * refresh cannot fix: the session is gone, and the app has to behave as
   * though the user never signed in.
   */
  clear(): void {
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(REFRESH_KEY);
    this._token.set(null);
    this._refresh.set(null);
  }

  private persist(r: AuthResponse): void {
    sessionStorage.setItem(TOKEN_KEY, r.token);
    sessionStorage.setItem(REFRESH_KEY, r.refreshToken);
    this._token.set(r.token);
    this._refresh.set(r.refreshToken);
  }
}
