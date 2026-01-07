export interface Notification {
    id: string;
    userId: number;
    message: string;
    type: 'Info' | 'Success' | 'Warning' | 'Error';
    isRead: boolean;
    createdAt: string;
}
