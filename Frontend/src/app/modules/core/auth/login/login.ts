import { Component } from '@angular/core';
import {FormBuilder, FormGroup, Validators} from '@angular/forms';
import {AuthLoginEndpointService} from '../../../../endpoints/auth-endpoints/auth-login-endpoint.service';
import {Router} from '@angular/router';
import {InputTextType} from '../../../shared/reactive-forms/input-text/input-text';


@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  form: FormGroup;

  constructor(private fb: FormBuilder, private authLoginService: AuthLoginEndpointService, private router: Router) {

    this.form = this.fb.group({
      email: ['string', [Validators.required, Validators.min(2), Validators.max(15)]],
      password: ['string', [Validators.required, Validators.min(2), Validators.max(30)]],
    });
  }

  onLogin(): void {
    if (this.form.invalid) return;

    this.authLoginService.handleAsync(this.form.value).subscribe({
      next: () => {
        console.log('Login successful');
      },
    });
  }

  protected readonly InputTextType = InputTextType;
}
