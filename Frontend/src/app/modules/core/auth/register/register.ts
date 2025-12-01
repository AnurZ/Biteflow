import { AfterViewInit, Component, ElementRef, OnDestroy, ViewChild, NgZone } from '@angular/core';
import { FormBuilder, Validators, FormGroup } from '@angular/forms';
import { AuthService } from '../../../../services/auth-services/auth.service';
import { hcaptchaConfig } from '../../../../../environments/environment.hcaptcha';

declare global {
  interface Window {
    hcaptcha: any;
  }
}

@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class RegisterComponent implements AfterViewInit, OnDestroy {
  form: FormGroup;
  captchaToken = '';
  captchaError = '';
  isSubmitting = false;
  errorMessage = '';
  successMessage = '';
  private captchaWidgetId: string | number | null = null;
  private readonly hcaptchaSiteKey = hcaptchaConfig.siteKey || '10000000-ffff-ffff-ffff-000000000001'; // replace with real key in environment.hcaptcha.ts
  private renderAttempts = 0;
  private rendered = false;

  @ViewChild('captchaRef', { static: false }) captchaRef?: ElementRef<HTMLDivElement>;

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly zone: NgZone
  ) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]],
      displayName: ['']
    }, { validators: this.passwordsMatchValidator });
  }

  get emailControl() {
    return this.form.get('email');
  }

  get passwordControl() {
    return this.form.get('password');
  }

  get confirmPasswordControl() {
    return this.form.get('confirmPassword');
  }

  get passwordStrength() {
    const pwd = this.passwordControl?.value ?? '';
    return this.computeStrength(pwd);
  }

  private passwordsMatchValidator(group: FormGroup): null | { passwordsMismatch: boolean } {
    const pwd = group.get('password')?.value ?? '';
    const confirm = group.get('confirmPassword')?.value ?? '';
    return pwd === confirm ? null : { passwordsMismatch: true };
  }

  onGoogleLogin(): void {
    this.authService.startGoogleLogin();
  }

  ngAfterViewInit(): void {
    // kick off render after view is initialized; retries until the widget appears or times out
    this.tryRenderCaptcha();
  }

  ngOnDestroy(): void {
    this.resetCaptcha();
  }

  private tryRenderCaptcha(): void {
    // If already rendered, nothing to do
    if (this.rendered) return;

    // If template not ready yet, retry shortly
    if (!this.captchaRef?.nativeElement) {
      if (this.renderAttempts < 30) {
        this.renderAttempts += 1;
        setTimeout(() => this.tryRenderCaptcha(), 150);
      }
      return;
    }

    if (window.hcaptcha && typeof window.hcaptcha.render === 'function') {
      try {
        this.captchaWidgetId = window.hcaptcha.render(this.captchaRef.nativeElement, {
          sitekey: this.hcaptchaSiteKey,
          callback: (token: string) => {
            this.zone.run(() => {
              this.captchaToken = token;
              this.captchaError = '';
            });
          },
          'expired-callback': () => {
            this.zone.run(() => {
              this.captchaToken = '';
              this.captchaError = 'Captcha expired. Please try again.';
            });
          },
          'error-callback': () => {
            this.zone.run(() => {
              this.captchaToken = '';
              this.captchaError = 'Captcha failed. Please retry.';
            });
          }
        });
        this.rendered = true;
      } catch (err) {
        console.error('hCaptcha render failed, retrying...', err);
        this.captchaWidgetId = null;
        if (this.renderAttempts < 30) {
          this.renderAttempts += 1;
          setTimeout(() => this.tryRenderCaptcha(), 150);
        } else {
          this.captchaError = 'Captcha failed to render. Please refresh.';
        }
      }
      return;
    }

    if (this.renderAttempts < 30) {
      this.renderAttempts += 1;
      setTimeout(() => this.tryRenderCaptcha(), 150);
    } else {
      this.captchaError = 'Captcha failed to load. Please disable blockers and refresh.';
    }
  }

  private resetCaptcha(): void {
    if (window.hcaptcha && this.captchaWidgetId !== null) {
      window.hcaptcha.reset(this.captchaWidgetId);
    }
    this.captchaToken = '';
  }

  private computeStrength(password: string): { score: number; label: string; color: string; width: string } {
    let score = 0;
    if (password.length >= 8) score += 1;
    if (/[A-Z]/.test(password) && /[a-z]/.test(password)) score += 1;
    if (/\d/.test(password)) score += 1;
    if (/[^A-Za-z0-9]/.test(password)) score += 1;

    const labels = ['Very weak', 'Weak', 'Fair', 'Good', 'Strong'];
    const colors = ['#c62828', '#ef6c00', '#fbc02d', '#7cb342', '#2e7d32'];
    const normalized = Math.min(score, 4);
    return {
      score,
      label: labels[normalized],
      color: colors[normalized],
      width: `${(normalized / 4) * 100}%`
    };
  }

  async onSubmit(): Promise<void> {
    if (this.isSubmitting) return;

    this.errorMessage = '';
    this.successMessage = '';

    if (this.form.invalid || !this.captchaToken) {
      this.form.markAllAsTouched();
      if (!this.captchaToken) {
        this.captchaError = 'Please complete the captcha.';
      }
      return;
    }

    this.isSubmitting = true;

    try {
      const payload = this.form.value as { email: string; password: string; displayName?: string };
      await this.authService.registerCustomer({ ...payload, captchaToken: this.captchaToken });
      this.successMessage = 'Account created. You can sign in now.';
    } catch (error: any) {
      console.error('Registration failed', error);
      this.errorMessage = error?.error?.message ?? 'Unable to register. Please try again.';
    } finally {
      this.isSubmitting = false;
    }
  }

  goToLogin(): void {
    this.authService.startLogin();
  }
}
