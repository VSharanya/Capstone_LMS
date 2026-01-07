import { Component, signal, ElementRef, HostListener, inject, OnInit, OnDestroy, computed } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatBadgeModule } from '@angular/material/badge';
import { NotificationService } from '../../../services/notification-service';

@Component({
    selector: 'app-notification-bell',
    standalone: true,
    imports: [
        CommonModule,
        MatIconModule,
        MatButtonModule,
        MatBadgeModule,
        MatTooltipModule
    ],
    templateUrl: './notification-bell.html',
    styleUrls: ['./notification-bell.css']
})
export class NotificationBellComponent implements OnInit, OnDestroy {

    private notificationService = inject(NotificationService);
    private elementRef = inject(ElementRef);
    private intervalId: any;

    isOpen = signal(false);
    // Convert Observable to Signal
    notifications = toSignal(this.notificationService.notifications$, { initialValue: [] });
    // Compute unread count from the notifications signal
    unreadCount = computed(() => this.notifications().filter(n => !n.isRead).length);

    constructor() { }

    ngOnInit() {
        // Initial load
        this.notificationService.loadNotifications();

        // Poll every 5 seconds for "Real-Time" feel
        this.intervalId = setInterval(() => {
            this.notificationService.loadNotifications();
        }, 5000);
    }

    ngOnDestroy() {
        if (this.intervalId) {
            clearInterval(this.intervalId);
        }
    }

    toggleOpen() {
        this.isOpen.update(v => !v);
    }

    markRead(id: string) {
        this.notificationService.markAsRead(id).subscribe();
    }

    markAllRead() {
        this.notificationService.markAllAsRead().subscribe();
    }

    @HostListener('document:click', ['$event'])
    onDocumentClick(event: MouseEvent) {
        if (!this.elementRef.nativeElement.contains(event.target)) {
            this.isOpen.set(false);
        }
    }
}
