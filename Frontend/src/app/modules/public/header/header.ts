import { Component, OnDestroy, OnInit } from '@angular/core';
import { AuthService } from '../../../services/auth-services/auth.service';
import { Router } from '@angular/router';
import { NotificationStoreService } from '../../../services/notifications/notification-store.service';
import { NotificationDto } from '../../../services/notifications/notification.model';
import { Observable, Subscription } from 'rxjs';

@Component({
  selector: 'app-header',
  standalone: false,
  templateUrl: './header.html',
  styleUrl: './header.css'
})
export class Header implements OnInit, OnDestroy {
  readonly unreadCount$: Observable<number>;
  readonly notifications$: Observable<NotificationDto[]>;
  panelOpen = false;
  private autoCloseHandle?: ReturnType<typeof setTimeout>;
  private sub = new Subscription();


  constructor(
    public authService: AuthService,
    public router: Router,
    private readonly notificationStore: NotificationStoreService
  ) {
    this.unreadCount$ = this.notificationStore.unreadCount$;
    this.notifications$ = this.notificationStore.notifications$;
  }

  ngOnInit(): void {
    this.notificationStore.init();
    this.sub.add(
      this.notificationStore.notificationReceived$.subscribe(() => {
        this.openPanelTemporarily();
      })
    );
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
    this.clearAutoClose();
  }

  get isAdmin(): boolean {
    const roles = this.authService.getRoles();
    return roles.includes('admin') || roles.includes('superadmin');
  }

  get isStaff(): boolean {
    const roles = this.authService.getRoles();
    return roles.includes('staff') || this.isAdmin;
  }

  isOnLoginPage():boolean{
    return this.router.url.includes('auth/login');
  }

  toggleNotifications(): void {
    this.panelOpen = !this.panelOpen;
    this.clearAutoClose();
  }

  closeNotifications(): void {
    this.panelOpen = false;
    this.clearAutoClose();
  }

  markAllRead(): void {
    this.notificationStore.markAllRead();
  }

  openNotification(notification: NotificationDto): void {
    if (!notification.readAtUtc) {
      this.notificationStore.markRead(notification.id);
    }

    this.panelOpen = false;
    this.clearAutoClose();
    if (notification.link) {
      this.router.navigateByUrl(notification.link);
    }
  }

  private openPanelTemporarily(): void {
    this.panelOpen = true;
    this.clearAutoClose();
    this.autoCloseHandle = setTimeout(() => {
      this.panelOpen = false;
      this.autoCloseHandle = undefined;
    }, 4000);
  }

  private clearAutoClose(): void {
    if (this.autoCloseHandle) {
      clearTimeout(this.autoCloseHandle);
      this.autoCloseHandle = undefined;
    }
  }

}
