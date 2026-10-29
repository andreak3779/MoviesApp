import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, defer, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { isAuthEndpoint } from './auth.interceptor';

/**
 * Translates common HTTP failures into user-visible navigation:
 *   401 → refresh the session once and retry, and if that is not possible
 *         clear the session and bounce to /login.
 *   403 → bubble up; routes guard against this.
 * Other errors bubble up untouched for component-level error rendering.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const auth = inject(AuthService);

  return next(req).pipe(
    catchError((err: unknown) => {
      if (!(err instanceof HttpErrorResponse) || err.status !== 401) {
        return throwError(() => err);
      }

      // A 401 from the login or refresh call is the answer to the question
      // that was asked, not an expired session.
      if (isAuthEndpoint(req.url)) {
        return throwError(() => err);
      }

      // The request went out without a token, so there is nothing to
      // refresh: the session is already gone.
      if (!req.headers.has('Authorization')) {
        auth.clear();
        return redirectToLogin(router, req.url, err);
      }

      // The access token has most likely expired. The refresh token is
      // single-use, so this is the one chance to trade it for a new session.
      // defer() turns a failure to even start the refresh — no session, or a
      // throw — into an ordinary error, so it lands in the same handler.
      return defer(() => auth.refreshSession()).pipe(
        switchMap((session) =>
          next(req.clone({ setHeaders: { Authorization: `Bearer ${session.token}` } })),
        ),
        catchError((refreshError: unknown) => {
          auth.clear();
          return redirectToLogin(router, req.url, refreshError);
        }),
      );
    }),
  );
};

/**
 * Sends the user to the login page, remembering where they came from, and
 * re-throws the failure so the caller still sees why the request failed.
 */
function redirectToLogin(router: Router, currentUrl: string, error: unknown) {
  if (!router.url.startsWith('/login')) {
    void router.navigate(['/login'], { queryParams: { returnUrl: currentUrl } });
  }

  return throwError(() => error);
}
