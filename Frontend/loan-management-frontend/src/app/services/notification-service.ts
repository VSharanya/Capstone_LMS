import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, timer } from 'rxjs';
import { switchMap, tap, retry, share } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface Notification {
    id: string;
    userId: number;
    message: string;
    type: string;
    isRead: boolean;
    createdAt: string;
}

// ----------------------------------------------------------------------------
// SERVICE: Notification
// ----------------------------------------------------------------------------

@Injectable({
    providedIn: 'root'
})
export class NotificationService {
    private apiUrl = `${environment.apiBaseUrl}/notification`;

    private notificationsSubject = new BehaviorSubject<Notification[]>([]);
    notifications$ = this.notificationsSubject.asObservable();

    // Polling subscription
    private pollingInterval = 30000; // 30 seconds

    constructor(private http: HttpClient) {
        this.startPolling();
    }

    // Load initial notifications
    loadNotifications() {
        this.http.get<Notification[]>(`${this.apiUrl}`).subscribe({
            next: (data) => this.notificationsSubject.next(data),
            error: (err) => console.error('Failed to load notifications', err)
        });
    }

    startPolling() {
        // Simple polling mechanism
        timer(0, this.pollingInterval).pipe(
            switchMap(() => this.http.get<Notification[]>(`${this.apiUrl}`)),
            retry(2),
            share()
        ).subscribe({
            next: (data) => this.notificationsSubject.next(data),
            error: (err) => console.log('Notification polling error', err)
        });
    }

    markAsRead(id: string): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/${id}/read`, {}).pipe(
            tap(() => {
                // Optimistic update
                const current = this.notificationsSubject.value;
                const updated = current.map(n => n.id === id ? { ...n, isRead: true } : n);
                this.notificationsSubject.next(updated);
            })
        );
    }

    markAllAsRead(): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/read-all`, {}).pipe(
            tap(() => {
                const current = this.notificationsSubject.value;
                const updated = current.map(n => ({ ...n, isRead: true }));
                this.notificationsSubject.next(updated);
            })
        );
    }

    getUnreadCount(): number {
        return this.notificationsSubject.value.filter(n => !n.isRead).length;
    }
}
