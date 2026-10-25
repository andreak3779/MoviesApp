import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { API_BASE_URL } from '../api-config';

/**
 * Shape of the JSON returned by {@code POST /api/v1/auth/login} and
 * {@code POST /api/v1/auth/refresh}. Matches the API's {@code AuthResponseDto}.
 */
export interface AuthResponse {
  token: string;
  refreshToken: string;
  expiration: string;
  userId?: string;
  email?: string;
}

export interface LoginPayload {
  email: string;
  password: string;
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
      .post<AuthResponse>(`${this.baseUrl}/auth/login`, payload)
      .pipe(tap((r) => this.persist(r)));
  }

  refresh(payload: RefreshPayload): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/auth/refresh`, payload)
      .pipe(tap((r) => this.persist(r)));
  }

  logout(): Observable<void> {
    const refresh = this._refresh();
    const headers = refresh ? { Authorization: `Bearer ${this._token() ?? ''}` } : undefined;
    return this.http
      .post<void>(`${this.baseUrl}/auth/logout`, { refreshToken: refresh }, { headers })
      .pipe(tap({ next: () => this.clear(), error: () => this.clear() }));
  }

  /** Synchronous read used by the auth interceptor. */
  getRefreshToken(): string | null {
    return this._refresh();
  }

  private persist(r: AuthResponse): void {
    sessionStorage.setItem(TOKEN_KEY, r.token);
    sessionStorage.setItem(REFRESH_KEY, r.refreshToken);
    this._token.set(r.token);
    this._refresh.set(r.refreshToken);
  }

  private clear(): void {
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(REFRESH_KEY);
    this._token.set(null);
    this._refresh.set(null);
  }
}
