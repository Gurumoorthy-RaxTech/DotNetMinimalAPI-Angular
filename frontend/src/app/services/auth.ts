// ============================================================
// AUTH SERVICE - Login, Logout, Token Management
// Tanglish: Idhu login/logout pannuvom, token store pannuvom
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
  username: string;
  role: string;
  expiresAt: string;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

// @Injectable decorator - DI container la register aagum
@Injectable({
  providedIn: 'root'  // Singleton - oru instance mattum
})
export class AuthService {

  private apiUrl = 'https://localhost:5260/api/v1/auth';

  // BehaviorSubject - login state track pannuvom
  // Components subscribe panlam - auto update aagum
  private isLoggedInSubject = new BehaviorSubject<boolean>(this.hasToken());
  public isLoggedIn$ = this.isLoggedInSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {}

  login(request: LoginRequest): Observable<ApiResponse<LoginResponse>> {
    return this.http.post<ApiResponse<LoginResponse>>(
      `${this.apiUrl}/login`, request
    ).pipe(
      tap(response => {
        if (response.success) {
          localStorage.setItem('token', response.data.token);
          localStorage.setItem('username', response.data.username);
          localStorage.setItem('role', response.data.role);
          this.isLoggedInSubject.next(true);
        }
      })
    );
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('username');
    localStorage.removeItem('role');
    this.isLoggedInSubject.next(false);
    this.router.navigate(['/login']);
  }

  hasToken(): boolean {
    return !!localStorage.getItem('token');
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  getUsername(): string {
    return localStorage.getItem('username') || '';
  }

  getRole(): string {
    return localStorage.getItem('role') || '';
  }

  isAdmin(): boolean {
    return this.getRole() === 'Admin';
  }
}
