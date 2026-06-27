import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard {
  username = '';
  role = '';

  constructor(private authService: AuthService) {
    this.username = this.authService.getUsername();
    this.role = this.authService.getRole();
  }
}
