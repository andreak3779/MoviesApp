import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

/**
 * Translates common HTTP failures into user-visible navigation:
 *   401 → clear session and bounce to /login (except on the login page itself).
 *   403 → bubble up; routes guard against this.
 * Other errors bubble up untouched for component-level error rendering.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const auth = inject(AuthService);

  return next(req).pipe(
    catchError((err: unknown) => {
      if (err instanceof HttpErrorResponse) {
        if (err.status === 401 && !req.url.includes('/auth/login')) {
          // Force-clear the session so the next render of the route guard
          // sends the user to /login.
          sessionStorage.removeItem('moviesapp.accessToken');
          sessionStorage.removeItem('moviesapp.refreshToken');
          if (!router.url.startsWith('/login')) {
            router.navigate(['/login'], { queryParams: { returnUrl: router.url } });
          }
        }
      }
      return throwError(() => err);
    }),
  );
};
