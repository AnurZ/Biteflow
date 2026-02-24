import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { Subscription } from 'rxjs';
import { AuthService } from '../../services/auth-services/auth.service';
import { DefaultLanding, UserSettingsService } from '../../services/settings/user-settings.service';

@Component({
  selector: 'app-settings',
  standalone: true,
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.css',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatSlideToggleModule,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule
  ]
})
export class SettingsComponent implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly settingsService = inject(UserSettingsService);
  private readonly sub = new Subscription();

  form = this.fb.group({
    autoOpenNotifications: this.fb.control(true, { nonNullable: true }),
    compactHeader: this.fb.control(false, { nonNullable: true }),
    notificationSound: this.fb.control(false, { nonNullable: true }),
    defaultLanding: this.fb.control<DefaultLanding>('auto', { nonNullable: true, validators: [Validators.required] })
  });

  readonly landingOptions: Array<{ value: DefaultLanding; label: string; enabled: boolean }> = [
    { value: 'auto', label: 'Auto (role-based)', enabled: true },
    { value: 'public', label: 'Public home', enabled: true },
    { value: 'admin', label: 'Admin dashboard', enabled: this.auth.hasRole('admin') && !this.auth.hasRole('superadmin') },
    { value: 'waiter', label: 'Waiter screen', enabled: this.auth.hasWaiterAccess() },
    { value: 'kitchen', label: 'Kitchen screen', enabled: this.auth.hasKitchenAccess() },
    { value: 'superadmin', label: 'Superadmin panel', enabled: this.auth.hasRole('superadmin') }
  ];

  ngOnInit(): void {
    this.sub.add(
      this.settingsService.settings$.subscribe((settings) => {
        this.form.patchValue(settings, { emitEvent: false });
      })
    );

    this.sub.add(
      this.form.valueChanges.subscribe((value) => {
        this.settingsService.update({
          autoOpenNotifications: value.autoOpenNotifications ?? true,
          compactHeader: value.compactHeader ?? false,
          notificationSound: value.notificationSound ?? false,
          defaultLanding: value.defaultLanding ?? 'auto'
        });
      })
    );
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  resetDefaults(): void {
    this.settingsService.reset();
  }
}
