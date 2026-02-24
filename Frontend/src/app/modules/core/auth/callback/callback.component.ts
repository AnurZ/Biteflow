import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../../services/auth-services/auth.service';
import { UserSettingsService } from '../../../../services/settings/user-settings.service';

@Component({
  selector: 'app-auth-callback',
  standalone: false,
  template: `
    <div class="callback-container">
      <h2>Completing sign-in...</h2>
      @if (error) {
        <p class="error">{{ error }}</p>
      } @else {
        <p>Please wait.</p>
      }
    </div>
  `,
  styles: [`
    .callback-container {
      max-width: 420px;
      margin: 4rem auto;
      padding: 2rem;
      text-align: center;
    }
    .error {
      color: #b91c1c;
      margin-top: 12px;
    }
  `]
})
export class AuthCallbackComponent implements OnInit {
  error: string | null = null;

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly userSettings: UserSettingsService
  ) {}

  async ngOnInit(): Promise<void> {
    try {
      await this.authService.handleLoginCallback();
      const requestedTarget = this.authService.getPostLoginRedirect();
      const target = requestedTarget && requestedTarget !== '/' && !requestedTarget.startsWith('/auth')
        ? requestedTarget
        : this.userSettings.getPreferredHomeRoute(this.authService);
      await this.router.navigateByUrl(target);
    } catch (err) {
      console.error('Login callback failed', err);
      this.error = 'Login failed. Please try again.';
    }
  }
}
