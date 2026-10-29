import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

/**
 * The auth endpoints the bearer token must not be attached to. A stale
 * token on a login request turns a valid sign-in into a failure, and a
 * stale token on a refresh request makes the refresh look like an
 * authenticated one when it is not.
 */
const AUTH_ENDPOINTS = ['/auth/login', '/auth/refresh'];

export function isAuthEndpoint(url: string): boolean {
  const lower = url.toLowerCase();
  return AUTH_ENDPOINTS.some((endpoint) => lower.includes(endpoint));
}

/**
 * Attaches the JWT bearer token to outgoing requests when one is present.
 * Skips auth endpoints so a stale token doesn't poison the login flow.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const token = auth.token();
  if (!token || isAuthEndpoint(req.url)) {
    return next(req);
  }
  return next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
};
