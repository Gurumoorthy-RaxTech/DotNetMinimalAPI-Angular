// ============================================================
// LOGIN COMPONENT - Login form logic
// Tanglish: Username, password enter panni login pannuvom
// ============================================================
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService, LoginRequest } from '../../services/auth';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule],  // Standalone component imports
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {

  // Two-way binding ku [(ngModel)] use aagum
  loginData: LoginRequest = {
    username: '',
    password: ''
  };

  isLoading = false;
  errorMessage = '';
  showPassword = false;

  constructor(private authService: AuthService, private router: Router) {
    // Already logged in aa check pannuvom
    if (this.authService.hasToken()) {
      this.router.navigate(['/dashboard']);
    }
  }

  onLogin(): void {
    if (!this.loginData.username || !this.loginData.password) {
      this.errorMessage = 'Please enter username and password';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.login(this.loginData).subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response.success) {
          this.router.navigate(['/dashboard']);
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.status === 401
          ? 'Invalid username or password'
          : 'Server error. Please try again.';
      }
    });
  }

  // Demo credentials fill pannuvom
  fillAdmin(): void {
    this.loginData = { username: 'admin', password: 'Admin@123' };
  }

  fillStudent(): void {
    this.loginData = { username: 'student', password: 'Student@123' };
  }
}
