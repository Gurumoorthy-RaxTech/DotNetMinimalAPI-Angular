// ============================================================
// ROOT APP COMPONENT - App shell with navbar
// ============================================================
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from './services/auth';

@Component({
  selector: 'app-root',
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  isLoggedIn = false;
  username = '';
  role = '';

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    // Subscribe to login state changes
    this.authService.isLoggedIn$.subscribe(loggedIn => {
      this.isLoggedIn = loggedIn;
      this.username = this.authService.getUsername();
      this.role = this.authService.getRole();
    });
  }

  logout(): void {
    this.authService.logout();
  }
}
