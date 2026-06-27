// ============================================================
// AUTH GUARD - Protected routes guard pannuvom
// Tanglish: Login aagaama dashboard access panna mudiyaathu
// ============================================================
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.hasToken()) {
    return true;  // Token irukku - access allow
  }

  // Token illai - login page redirect pannuvom
  router.navigate(['/login']);
  return false;
};
