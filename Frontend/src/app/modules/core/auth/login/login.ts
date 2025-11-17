import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { InputTextType } from '../../../shared/reactive-forms/input-text/input-text';
import { AuthService } from '../../../../services/auth-services/auth.service';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  form: FormGroup;
  loginError = '';
  isLoading = false;

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly router: Router
  ) {
    this.form = this.fb.group({
      email: ['string', [Validators.required]],
      password: ['string', [Validators.required]]
    });
  }

  onLogin(): void {
    if (this.form.invalid || this.isLoading) return;

    this.isLoading = true;
    this.loginError = '';

    const email = String(this.form.value.email ?? '').trim();
    const password = String(this.form.value.password ?? '');

    this.authService.login(email, password).subscribe({
      next: () => {
        this.router.navigate(['/public']);
      },
      error: (error) => {
        console.error('Login failed', error);
        this.loginError = 'Invalid credentials or server error. Please try again.';
        this.isLoading = false;
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }

  protected readonly InputTextType = InputTextType;
}
