// ============================================================
// JWT INTERCEPTOR - Auto token attach + Auto refresh on 401
//
// FLOW (Interview la explain pannuvom):
// 1. Every request la → Bearer token attach pannuvom
// 2. Server 401 return pannuvom (token expired)
// 3. Interceptor catch pannuvom → refresh API call
// 4. New token get pannuvom → original request retry pannuvom
// 5. Refresh also fail → logout pannuvom
//
// User ku teriyadhe! Automatic background la nadakkum.
// ============================================================
import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth';

export const jwtInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const authService = inject(AuthService);

  // Refresh endpoint itself la token add pannidathe — infinite loop aagum!
  if (req.url.includes('/auth/refresh') || req.url.includes('/auth/login')) {
    return next(req);
  }

  // Step 1: Access token attach pannuvom
  const authReq = attachToken(req, authService.getToken());

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {

      // Step 2: 401 = token expired or invalid
      if (error.status === 401) {

        // Step 3: Refresh token irukka?
        const refreshToken = authService.getRefreshToken();
        if (!refreshToken) {
          // Refresh token illai → logout
          authService.logout();
          return throwError(() => error);
        }

        // Step 4: Refresh API call pannuvom
        return authService.refreshTokens().pipe(
          switchMap(response => {
            if (response.success) {
              // Step 5: New token kooda original request retry pannuvom
              const retryReq = attachToken(req, response.data.token);
              return next(retryReq);
            }
            // Refresh failed → logout
            authService.logout();
            return throwError(() => error);
          }),
          catchError(refreshError => {
            // Refresh API itself failed (network error, 401, etc.)
            authService.logout();
            return throwError(() => refreshError);
          })
        );
      }

      // Other errors (403, 404, 500) → pass through
      return throwError(() => error);
    })
  );
};

// Helper: Request clone pannuvom with Bearer token
function attachToken(req: HttpRequest<unknown>, token: string | null): HttpRequest<unknown> {
  if (!token) return req;
  return req.clone({
    setHeaders: { Authorization: `Bearer ${token}` }
  });
}
