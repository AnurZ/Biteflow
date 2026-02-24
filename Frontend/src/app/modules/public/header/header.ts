import { Component, OnDestroy, OnInit } from '@angular/core';
import { AuthService } from '../../../services/auth-services/auth.service';
import { Router } from '@angular/router';
import { NotificationStoreService } from '../../../services/notifications/notification-store.service';
import { NotificationDto } from '../../../services/notifications/notification.model';
import { Observable, Subscription } from 'rxjs';
import { UserSettingsService } from '../../../services/settings/user-settings.service';

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
  compactHeader = false;
  private autoCloseHandle?: ReturnType<typeof setTimeout>;
  private sub = new Subscription();


  constructor(
    public authService: AuthService,
    public router: Router,
    private readonly notificationStore: NotificationStoreService,
    private readonly userSettings: UserSettingsService
  ) {
    this.unreadCount$ = this.notificationStore.unreadCount$;
    this.notifications$ = this.notificationStore.notifications$;
  }

  ngOnInit(): void {
    this.notificationStore.init();
    this.sub.add(
      this.userSettings.settings$.subscribe((settings) => {
        this.compactHeader = settings.compactHeader;
      })
    );

    this.sub.add(
      this.notificationStore.notificationReceived$.subscribe(() => {
        if (this.userSettings.snapshot.notificationSound) {
          this.playNotificationSound();
        }

        if (this.userSettings.snapshot.autoOpenNotifications) {
          this.openPanelTemporarily();
        }
      })
    );
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
    this.clearAutoClose();
  }

  get isSuperAdmin(): boolean {
    const roles = this.authService.getRoles();
    return roles.includes('superadmin');
  }

  get isRestaurantAdmin(): boolean {
    const roles = this.authService.getRoles();
    return roles.includes('admin') && !roles.includes('superadmin');
  }

  get isStaff(): boolean {
    if (this.isSuperAdmin) return false;
    return this.authService.hasRole('staff') || this.isRestaurantAdmin;
  }

  get canAccessWaiter(): boolean {
    if (!this.authService.isLoggedIn() || this.isSuperAdmin) return false;
    return this.authService.hasWaiterAccess() || this.authService.hasRole('staff') || this.isRestaurantAdmin;
  }

  get canAccessKitchen(): boolean {
    if (!this.authService.isLoggedIn() || this.isSuperAdmin) return false;
    return this.authService.hasKitchenAccess() || this.authService.hasRole('staff') || this.isRestaurantAdmin;
  }

  get brandName(): string {
    if (!this.authService.isLoggedIn()) return 'Biteflow';
    if (this.isSuperAdmin) return 'Biteflow Admin';

    const tenantName = this.authService.getTenantName().trim();
    return tenantName.length > 0 ? tenantName : 'Biteflow';
  }

  isOnLoginPage():boolean{
    return this.router.url.includes('auth/login');
  }

  goHome(): void {
    const target = this.userSettings.getPreferredHomeRoute(this.authService);
    this.router.navigateByUrl(target);
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

  private playNotificationSound(): void {
    try {
      const context = new AudioContext();
      const oscillator = context.createOscillator();
      const gain = context.createGain();

      oscillator.type = 'triangle';
      oscillator.frequency.value = 940;
      gain.gain.value = 0.04;

      oscillator.connect(gain);
      gain.connect(context.destination);

      oscillator.start();
      oscillator.stop(context.currentTime + 0.12);
      oscillator.onended = () => context.close();
    } catch {
      // No-op when audio context is not available (browser policies/private mode).
    }
  }

}
