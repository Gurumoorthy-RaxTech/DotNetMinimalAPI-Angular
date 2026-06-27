// ============================================================
// REALTIME DASHBOARD COMPONENT
// Tanglish: SignalR use panni live stats, activity feed,
// notifications show pannuvom - page refresh pannama update aagum!
// ============================================================
import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { SignalRService, DashboardStats, ActivityLog, Notification, ConnectionState } from '../../services/signalr.service';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-realtime-dashboard',
  imports: [CommonModule, FormsModule],
  templateUrl: './realtime-dashboard.html',
  styleUrl: './realtime-dashboard.scss'
})
export class RealtimeDashboard implements OnInit, OnDestroy {

  stats: DashboardStats | null = null;
  activities: ActivityLog[] = [];
  notifications: Notification[] = [];
  connectionState: ConnectionState = 'disconnected';
  connectedUsers = 0;
  username = '';
  broadcastMessage = '';

  // Chart data - last 10 data points
  cpuHistory: number[] = Array(10).fill(0);
  memHistory: number[] = Array(10).fill(0);

  private subs: Subscription[] = [];

  constructor(
    private signalRService: SignalRService,
    private authService: AuthService
  ) {
    this.username = this.authService.getUsername();
  }

  ngOnInit(): void {
    // SignalR connection start pannuvom
    this.signalRService.startConnection();

    // Stats subscribe pannuvom - every 2s update
    this.subs.push(
      this.signalRService.stats$.subscribe(stats => {
        if (stats) {
          this.stats = stats;
          // Chart history update pannuvom
          this.cpuHistory = [...this.cpuHistory.slice(1), Math.round(stats.cpuUsage)];
          this.memHistory = [...this.memHistory.slice(1), stats.memoryUsageMb];
        }
      })
    );

    // Activity feed subscribe
    this.subs.push(
      this.signalRService.activity$.subscribe(activity => {
        this.activities = [activity, ...this.activities].slice(0, 20); // max 20 items
      })
    );

    // Notifications subscribe - auto dismiss after 5s
    this.subs.push(
      this.signalRService.notification$.subscribe(notification => {
        this.notifications = [notification, ...this.notifications].slice(0, 5);
        setTimeout(() => {
          this.notifications = this.notifications.filter(n => n.id !== notification.id);
        }, 5000);
      })
    );

    // Connection state
    this.subs.push(
      this.signalRService.connectionState$.subscribe(state => {
        this.connectionState = state;
      })
    );

    // Connected users
    this.subs.push(
      this.signalRService.connectedUsers$.subscribe(count => {
        this.connectedUsers = count;
      })
    );
  }

  sendPing(): void {
    this.signalRService.ping('Hello from ' + this.username);
  }

  sendBroadcast(): void {
    if (this.broadcastMessage.trim()) {
      this.signalRService.broadcastMessage(this.broadcastMessage);
      this.broadcastMessage = '';
    }
  }

  dismissNotification(id: string): void {
    this.notifications = this.notifications.filter(n => n.id !== id);
  }

  getBarHeight(value: number, max: number): string {
    return Math.round((value / max) * 60) + 'px';
  }

  getConnectionClass(): string {
    const map: Record<ConnectionState, string> = {
      connected: 'state-connected',
      connecting: 'state-connecting',
      reconnecting: 'state-reconnecting',
      disconnected: 'state-disconnected'
    };
    return map[this.connectionState];
  }

  getConnectionIcon(): string {
    const map: Record<ConnectionState, string> = {
      connected: '🟢', connecting: '🟡', reconnecting: '🟠', disconnected: '🔴'
    };
    return map[this.connectionState];
  }

  ngOnDestroy(): void {
    this.subs.forEach(s => s.unsubscribe());
    this.signalRService.stopConnection();
  }
}
