
export type NotificationType = 'success' | 'error' | 'info';

export interface UserNotification {
    id: number;
    type: NotificationType;
    message: string;
}