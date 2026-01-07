
import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService } from '../../../services/auth-service';
import { NotificationBellComponent } from '../notification-bell/notification-bell';

@Component({
    selector: 'app-sidebar',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        MatListModule,
        MatIconModule,
        MatButtonModule,
        MatDividerModule,
        NotificationBellComponent
    ],
    templateUrl: './sidebar.html',
    styleUrls: ['./sidebar.css']
})
export class SidebarComponent {
    isLoggedIn = signal(false);
    role = signal('');
    currentUser = signal('');

    constructor(private authService: AuthService, private router: Router) {
        this.authService.authStatus$.subscribe(isLoggedIn => {
            this.updateAuthState(isLoggedIn);
        });
    }

    updateAuthState(isLoggedIn: boolean) {
        this.isLoggedIn.set(isLoggedIn);
        if (isLoggedIn) {
            this.role.set(this.authService.getUserRole());
            this.currentUser.set(this.authService.getUserName() || 'User');
        } else {
            this.role.set('');
            this.currentUser.set('');
        }
    }

    logout() {
        this.authService.logout();
        this.router.navigate(['/auth/login']);
    }
}
