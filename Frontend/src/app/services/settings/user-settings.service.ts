import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { AuthService } from '../auth-services/auth.service';

export type DefaultLanding = 'auto' | 'public' | 'admin' | 'waiter' | 'kitchen' | 'superadmin';

export interface UserSettings {
  autoOpenNotifications: boolean;
  compactHeader: boolean;
  notificationSound: boolean;
  defaultLanding: DefaultLanding;
}

@Injectable({
  providedIn: 'root'
})
export class UserSettingsService {
  private readonly storageKey = 'biteflow.user-settings.v1';
  private readonly defaults: UserSettings = {
    autoOpenNotifications: true,
    compactHeader: false,
    notificationSound: false,
    defaultLanding: 'auto'
  };

  private readonly settingsSubject = new BehaviorSubject<UserSettings>(this.load());
  readonly settings$ = this.settingsSubject.asObservable();

  get snapshot(): UserSettings {
    return this.settingsSubject.value;
  }

  update(patch: Partial<UserSettings>): void {
    const next = { ...this.snapshot, ...patch };
    this.settingsSubject.next(next);
    this.persist(next);
  }

  reset(): void {
    this.settingsSubject.next(this.defaults);
    this.persist(this.defaults);
  }

  getPreferredHomeRoute(auth: AuthService): string {
    const preferred = this.snapshot.defaultLanding;

    if (!auth.isLoggedIn()) {
      return '/public';
    }

    switch (preferred) {
      case 'public':
        return '/public';
      case 'admin':
        return '/admin';
      case 'waiter':
        return '/waiter';
      case 'kitchen':
        return '/kitchen';
      case 'superadmin':
        return '/superadmin';
      case 'auto':
      default:
        return this.autoRoute(auth);
    }
  }

  private autoRoute(auth: AuthService): string {
    const isSuperAdmin = auth.hasRole('superadmin');
    const isAdmin = auth.hasRole('admin') && !isSuperAdmin;
    const isStaff = auth.hasRole('staff');

    if (isSuperAdmin) return '/superadmin';
    if (isAdmin) return '/admin';
    if (auth.hasWaiterAccess() || isStaff) return '/waiter';
    if (auth.hasKitchenAccess() || isStaff) return '/kitchen';
    return '/public';
  }

  private load(): UserSettings {
    try {
      const raw = localStorage.getItem(this.storageKey);
      if (!raw) return this.defaults;

      const parsed = JSON.parse(raw) as Partial<UserSettings>;
      return {
        autoOpenNotifications: parsed.autoOpenNotifications ?? this.defaults.autoOpenNotifications,
        compactHeader: parsed.compactHeader ?? this.defaults.compactHeader,
        notificationSound: parsed.notificationSound ?? this.defaults.notificationSound,
        defaultLanding: parsed.defaultLanding ?? this.defaults.defaultLanding
      };
    } catch {
      return this.defaults;
    }
  }

  private persist(value: UserSettings): void {
    try {
      localStorage.setItem(this.storageKey, JSON.stringify(value));
    } catch {
      // Ignore storage failures (private mode/full storage), keep runtime state alive.
    }
  }
}
