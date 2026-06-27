// ============================================================
// SIGNALR SERVICE - Real-time connection management
// Tanglish: Server kooda WebSocket connection maintain pannuvom
// Auto-reconnect, JWT token attach, event listeners setup
// ============================================================
import { Injectable, OnDestroy } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Subject } from 'rxjs';
import { AuthService } from './auth';

export interface DashboardStats {
  totalStudents: number;
  connectedUsers: number;
  totalCourses: number;
  serverTime: string;
  cpuUsage: number;
  memoryUsageMb: number;
}

export interface ActivityLog {
  id: string;
  action: string;
  message: string;
  user: string;
  timestamp: string;
  type: string;
}

export interface Notification {
  id: string;
  title: string;
  message: string;
  type: string;
  timestamp: string;
}

export type ConnectionState = 'disconnected' | 'connecting' | 'connected' | 'reconnecting';

@Injectable({ providedIn: 'root' })
export class SignalRService implements OnDestroy {

  private hubConnection!: signalR.HubConnection;
  private readonly hubUrl = 'https://localhost:5260/hubs/dashboard';

  // BehaviorSubjects - components subscribe pannalam
  private statsSubject = new BehaviorSubject<DashboardStats | null>(null);
  private activitySubject = new Subject<ActivityLog>();
  private notificationSubject = new Subject<Notification>();
  private connectedUsersSubject = new BehaviorSubject<number>(0);
  private connectionStateSubject = new BehaviorSubject<ConnectionState>('disconnected');

  // Public observables - components read pannuvom
  public stats$ = this.statsSubject.asObservable();
  public activity$ = this.activitySubject.asObservable();
  public notification$ = this.notificationSubject.asObservable();
  public connectedUsers$ = this.connectedUsersSubject.asObservable();
  public connectionState$ = this.connectionStateSubject.asObservable();

  constructor(private authService: AuthService) {}

  // HubConnection build pannuvom
  startConnection(): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        // JWT token attach pannuvom - server validate pannuvom
        accessTokenFactory: () => this.authService.getToken() ?? '',
        transport: signalR.HttpTransportType.WebSockets // WebSocket prefer
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // Reconnect intervals
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.registerHandlers();
    this.registerConnectionEvents();
    this.connect();
  }

  // Server -> Client event handlers register pannuvom
  private registerHandlers(): void {

    // Every 2 seconds server stats send pannuvom
    this.hubConnection.on('StatsUpdated', (stats: DashboardStats) => {
      this.statsSubject.next(stats);
    });

    // Activity feed events
    this.hubConnection.on('ActivityReceived', (activity: ActivityLog) => {
      this.activitySubject.next(activity);
    });

    // Toast notifications
    this.hubConnection.on('ReceiveNotification', (notification: Notification) => {
      this.notificationSubject.next(notification);
    });

    // Connected users count update
    this.hubConnection.on('ConnectedUsersUpdated', (count: number) => {
      this.connectedUsersSubject.next(count);
    });

    // Pong response from server
    this.hubConnection.on('Pong', (message: string) => {
      console.log('Pong from server:', message);
    });

    // Broadcast messages
    this.hubConnection.on('ReceiveMessage', (data: any) => {
      console.log('Broadcast message:', data);
    });
  }

  // Connection lifecycle events
  private registerConnectionEvents(): void {
    this.hubConnection.onreconnecting(() => {
      this.connectionStateSubject.next('reconnecting');
    });

    this.hubConnection.onreconnected(() => {
      this.connectionStateSubject.next('connected');
    });

    this.hubConnection.onclose(() => {
      this.connectionStateSubject.next('disconnected');
    });
  }

  private connect(): void {
    this.connectionStateSubject.next('connecting');

    this.hubConnection.start()
      .then(() => {
        this.connectionStateSubject.next('connected');
        console.log('SignalR connected!');
      })
      .catch(err => {
        this.connectionStateSubject.next('disconnected');
        console.error('SignalR connection failed:', err);
      });
  }

  // Client -> Server method invoke pannuvom
  ping(message: string): void {
    if (this.isConnected()) {
      this.hubConnection.invoke('Ping', message);
    }
  }

  broadcastMessage(message: string): void {
    if (this.isConnected()) {
      this.hubConnection.invoke('BroadcastMessage', message);
    }
  }

  stopConnection(): void {
    this.hubConnection?.stop();
  }

  isConnected(): boolean {
    return this.hubConnection?.state === signalR.HubConnectionState.Connected;
  }

  ngOnDestroy(): void {
    this.stopConnection();
  }
}
