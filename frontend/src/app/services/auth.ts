// ============================================================
// AUTH SERVICE - Login, Logout, Token Management, Refresh Token
// ============================================================
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;          // NEW — 7 days validity
  username: string;
  role: string;
  expiresAt: string;
  refreshTokenExpiresAt: string; // NEW
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

export interface RefreshTokenRequest {
  accessToken: string;
  refreshToken: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {

  private apiUrl = 'https://localhost:5260/api/v1/auth';

  private isLoggedInSubject = new BehaviorSubject<boolean>(this.hasToken());
  public isLoggedIn$ = this.isLoggedInSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {}

  // ── LOGIN ──────────────────────────────────────────────────
  login(request: LoginRequest): Observable<ApiResponse<LoginResponse>> {
    return this.http.post<ApiResponse<LoginResponse>>(`${this.apiUrl}/login`, request)
      .pipe(
        tap(response => {
          if (response.success) {
            this.storeTokens(response.data);
            this.isLoggedInSubject.next(true);
          }
        })
      );
  }

  // ── REFRESH TOKENS ─────────────────────────────────────────
  // Access token expire aana interceptor idha call pannuvom
  refreshTokens(): Observable<ApiResponse<LoginResponse>> {
    const request: RefreshTokenRequest = {
      accessToken: this.getToken() ?? '',
      refreshToken: this.getRefreshToken() ?? ''
    };

    return this.http.post<ApiResponse<LoginResponse>>(`${this.apiUrl}/refresh`, request)
      .pipe(
        tap(response => {
          if (response.success) {
            this.storeTokens(response.data); // New tokens store pannuvom
          }
        })
      );
  }

  // ── LOGOUT ─────────────────────────────────────────────────
  logout(): void {
    const refreshToken = this.getRefreshToken();

    // Server la refresh token revoke pannuvom
    if (refreshToken) {
      this.http.post(`${this.apiUrl}/revoke`, null, {
        headers: { 'X-Refresh-Token': refreshToken }
      }).subscribe({ error: () => {} }); // Fire and forget
    }

    this.clearTokens();
    this.isLoggedInSubject.next(false);
    this.router.navigate(['/login']);
  }

  // ── STORE / CLEAR ──────────────────────────────────────────
  private storeTokens(data: LoginResponse): void {
    localStorage.setItem('token', data.token);
    localStorage.setItem('refreshToken', data.refreshToken);
    localStorage.setItem('username', data.username);
    localStorage.setItem('role', data.role);
    localStorage.setItem('expiresAt', data.expiresAt);
  }

  private clearTokens(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('username');
    localStorage.removeItem('role');
    localStorage.removeItem('expiresAt');
  }

  // ── GETTERS ────────────────────────────────────────────────
  hasToken(): boolean       { return !!localStorage.getItem('token'); }
  getToken(): string | null { return localStorage.getItem('token'); }
  getRefreshToken(): string | null { return localStorage.getItem('refreshToken'); }
  getUsername(): string     { return localStorage.getItem('username') || ''; }
  getRole(): string         { return localStorage.getItem('role') || ''; }
  isAdmin(): boolean        { return this.getRole() === 'Admin'; }

  // Access token expire aagiruchaa check pannuvom
  isTokenExpired(): boolean {
    const expiresAt = localStorage.getItem('expiresAt');
    if (!expiresAt) return true;
    return new Date(expiresAt) <= new Date();
  }
}
