import { Injectable } from '@angular/core';
import { BehaviorSubject, Subject, Subscription, map } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { NotificationApiService } from './notification-api.service';
import { NotificationDto, NotificationListRequest } from './notification.model';
import { RealtimeHubService } from '../realtime/realtime-hub.service';
import { MyConfig } from '../../my-config';
import { AuthService } from '../auth-services/auth.service';

interface NotificationState {
  notifications: NotificationDto[];
  unreadCount: number;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationStoreService {
  private state$ = new BehaviorSubject<NotificationState>({
    notifications: [],
    unreadCount: 0
  });
  private received$ = new Subject<NotificationDto>();

  readonly notifications$ = this.state$.asObservable().pipe(map(state => state.notifications));
  readonly unreadCount$ = this.state$.asObservable().pipe(map(state => state.unreadCount));

  private initialized = false;
  private realtimeSub?: Subscription;
  private authWaitHandle?: ReturnType<typeof setInterval>;

  constructor(
    private readonly api: NotificationApiService,
    private readonly realtime: RealtimeHubService,
    private readonly snack: MatSnackBar,
    private readonly auth: AuthService
  ) {}

  get notificationReceived$() {
    return this.received$.asObservable();
  }

  init(): void {
    if (this.initialized) return;
    if (!this.auth.isLoggedIn()) {
      this.startAuthWait();
      return;
    }

    this.initialized = true;
    this.loadInitial();
    this.subscribeToRealtime();
  }

  loadInitial(request: NotificationListRequest = { pageNumber: 1, pageSize: 20, unreadOnly: true }): void {
    this.api.list(request).subscribe({
      next: result => {
        this.state$.next({
          notifications: result.items ?? [],
          unreadCount: result.unreadCount ?? 0
        });
      }
    });
  }

  subscribeToRealtime(): void {
    if (this.realtimeSub) return;
    void this.realtime.start(MyConfig.orders_hub);

    this.realtimeSub = new Subscription();

    this.realtimeSub.add(
      this.realtime
        .on<NotificationDto>('NotificationCreated')
        .subscribe(notification => {
          const state = this.state$.value;
          const alreadyExists = state.notifications.some(item => item.id === notification.id);
          const notifications = [notification, ...state.notifications]
            .filter((item, index, self) => self.findIndex(x => x.id === item.id) === index)
            .slice(0, 50);

          this.state$.next({
            notifications,
            unreadCount: state.unreadCount + (alreadyExists ? 0 : 1)
          });

          this.received$.next(notification);

          this.snack.open(notification.title || 'Nova notifikacija', 'Close', {
            duration: 2500,
            panelClass: ['app-snackbar', 'app-snackbar-info']
          });
        })
    );

    this.realtimeSub.add(
      this.realtime
        .on<{ orderId?: number; notificationIds?: number[] }>('NotificationCleared')
        .subscribe(({ orderId, notificationIds }) => {
          const state = this.state$.value;
          const ids = Array.isArray(notificationIds) ? notificationIds : [];
          let notifications = state.notifications;
          let unreadRemoved = 0;

          if (ids.length > 0) {
            notifications = notifications.filter(item => {
              const match = ids.includes(item.id);
              if (match && !item.readAtUtc) {
                unreadRemoved += 1;
              }
              return !match;
            });
          }

          if (orderId) {
            const result = this.removeByOrderId(notifications, orderId);
            notifications = result.notifications;
            unreadRemoved += result.unreadRemoved;
          }

          if (notifications.length === state.notifications.length && unreadRemoved === 0) {
            return;
          }

          this.state$.next({
            notifications,
            unreadCount: Math.max(0, state.unreadCount - unreadRemoved)
          });
        })
    );
  }

  markRead(id: number): void {
    this.api.markRead(id).subscribe({
      next: () => {
        const state = this.state$.value;
        const updated = state.notifications.filter(item => item.id !== id);
        const unreadCount = Math.max(0, state.unreadCount - 1);
        this.state$.next({
          notifications: updated,
          unreadCount
        });
      }
    });
  }

  markAllRead(): void {
    const state = this.state$.value;
    if (state.notifications.length === 0 && state.unreadCount === 0) {
      return;
    }

    this.state$.next({
      notifications: [],
      unreadCount: 0
    });

    this.api.markAllRead().subscribe({
      next: () => void 0,
      error: () => this.loadInitial()
    });
  }

  private removeByOrderId(items: NotificationDto[], orderId: number): { notifications: NotificationDto[]; unreadRemoved: number } {
    let unreadRemoved = 0;
    const notifications = items.filter(item => {
      const itemOrderId = this.extractOrderId(item.link);
      const match = itemOrderId === orderId;
      if (match && !item.readAtUtc) {
        unreadRemoved += 1;
      }
      return !match;
    });

    return { notifications, unreadRemoved };
  }

  private extractOrderId(link?: string): number | null {
    if (!link) return null;
    const match = link.match(/\/(\d+)(\D|$)/);
    if (!match) return null;
    const parsed = Number(match[1]);
    return Number.isFinite(parsed) ? parsed : null;
  }

  private startAuthWait(): void {
    if (this.authWaitHandle) return;
    this.authWaitHandle = setInterval(() => {
      if (!this.auth.isLoggedIn()) {
        return;
      }

      if (this.authWaitHandle) {
        clearInterval(this.authWaitHandle);
        this.authWaitHandle = undefined;
      }
      this.init();
    }, 1000);
  }
}
