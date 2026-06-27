// ============================================================
// APP CONFIG - Angular application level configuration
// Tanglish: App-wide settings - HttpClient, Router, Interceptors
// ============================================================
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { routes } from './app.routes';
import { jwtInterceptor } from './interceptors/jwt-interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),

    // Router configure pannuvom - routes register
    provideRouter(routes),

    // HttpClient configure pannuvom - JWT interceptor add pannuvom
    // withInterceptors - every HTTP call la interceptor run aagum
    provideHttpClient(
      withInterceptors([jwtInterceptor])  // JWT token auto-attach
    )
  ]
};
