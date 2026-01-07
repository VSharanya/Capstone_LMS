import { Component, computed, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { MatBadgeModule } from '@angular/material/badge';

import { AuthService } from '../../../services/auth-service';
import { NotificationService, Notification } from '../../../services/notification-service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatDividerModule,
    MatBadgeModule
  ],
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.css']
})
export class NavbarComponent {

  isLoggedIn = signal(false);
  role = signal('');
  currentUser = signal('');

  // Notifications
  notifications = signal<Notification[]>([]);
  unreadCount = computed(() => this.notifications().filter(n => !n.isRead).length);

  constructor(
    private authService: AuthService,
    private router: Router,
    private notificationService: NotificationService
  ) {
    // Reactive auth state
    this.authService.authStatus$.subscribe(isLoggedIn => {
      this.updateAuthState(isLoggedIn);
    });

    // Subscribe to notifications
    this.notificationService.notifications$.subscribe(check => {
      this.notifications.set(check);
    });
  }

  updateAuthState(isLoggedIn: boolean) {
    this.isLoggedIn.set(isLoggedIn);
    if (isLoggedIn) {
      this.role.set(this.authService.getUserRole());
      this.currentUser.set(this.authService.getUserName() || 'User');

      // Start polling/loading notifications on login
      // The service constructor already started polling, but it might be empty if not logged in.
      // Ideally we trigger reload here.
      this.notificationService.loadNotifications();
    } else {
      this.role.set('');
      this.currentUser.set('');
      this.notifications.set([]);
    }
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }

  markRead(n: Notification) {
    if (!n.isRead) {
      this.notificationService.markAsRead(n.id).subscribe();
    }
  }

  markAllRead() {
    this.notificationService.markAllAsRead().subscribe();
  }

  getIcon(type: string): string {
    switch (type) {
      case 'Success': return 'check_circle';
      case 'Error': return 'error';
      case 'Warning': return 'warning';
      default: return 'info';
    }
  }
}
