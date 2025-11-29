import { Component } from '@angular/core';
import { FormBuilder, Validators, FormGroup } from '@angular/forms';
import { AuthService } from '../../../../services/auth-services/auth.service';

@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class RegisterComponent {
  form: FormGroup;
  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService
  ) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      displayName: ['']
    });
  }

  get emailControl() {
    return this.form.get('email');
  }

  get passwordControl() {
    return this.form.get('password');
  }

  onGoogleLogin(): void {
    this.authService.startGoogleLogin();
  }
  isSubmitting = false;
  errorMessage = '';
  successMessage = '';



  async onSubmit(): Promise<void> {
    if (this.isSubmitting) return;

    this.errorMessage = '';
    this.successMessage = '';

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;

    try {
      const payload = this.form.value as { email: string; password: string; displayName?: string };
      await this.authService.registerCustomer(payload);
      this.successMessage = 'Account created. Continue to sign in.';
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
