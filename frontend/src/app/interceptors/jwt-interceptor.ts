// ============================================================
// JWT INTERCEPTOR - Every HTTP request la token attach pannuvom
// Tanglish: Automatically every API call la "Authorization: Bearer token" add aagum
// Manual aa har oru call la token add pannanum illai
// ============================================================
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  if (token) {
    // Request clone pannuvom - original immutable
    const authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`  // JWT token attach pannuvom
      }
    });
    return next(authReq);  // Modified request send pannuvom
  }

  return next(req);  // Token illai - original request send
};
