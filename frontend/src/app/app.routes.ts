// ============================================================
// ROUTES - URL -> Component mapping
// Tanglish: URL type pannum pothu enna component show aagum
// ============================================================
import { Routes } from '@angular/router';
import { authGuard } from './guards/auth-guard';

export const routes: Routes = [
  // Default route - login page
  { path: '', redirectTo: 'login', pathMatch: 'full' },

  // Login page - no guard needed
  {
    path: 'login',
    loadComponent: () => import('./components/login/login').then(m => m.Login)
  },

  // Dashboard - protected by authGuard
  {
    path: 'dashboard',
    loadComponent: () => import('./components/dashboard/dashboard').then(m => m.Dashboard),
    canActivate: [authGuard]  // Login aagaama access mudiyathu
  },

  // Students - protected by authGuard
  {
    path: 'students',
    loadComponent: () => import('./components/students/students').then(m => m.Students),
    canActivate: [authGuard]
  },

  // Realtime SignalR Dashboard
  {
    path: 'realtime',
    loadComponent: () => import('./components/realtime-dashboard/realtime-dashboard').then(m => m.RealtimeDashboard),
    canActivate: [authGuard]
  },

  // Wildcard - 404 page
  { path: '**', redirectTo: 'login' }
];
