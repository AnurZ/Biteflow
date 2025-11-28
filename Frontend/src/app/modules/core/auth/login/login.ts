import { Component } from '@angular/core';
import { AuthService } from '../../../../services/auth-services/auth.service';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  loginError = '';
  isLoading = false;

  constructor(private readonly authService: AuthService) {}

  onLogin(): void {
    if (this.isLoading) return;

    this.isLoading = true;
    this.loginError = '';

    try {
      this.authService.startLogin();
    } catch (error) {
      console.error('Login redirect failed', error);
      this.loginError = 'Unable to start login. Please try again.';
      this.isLoading = false;
    }
  }
}
