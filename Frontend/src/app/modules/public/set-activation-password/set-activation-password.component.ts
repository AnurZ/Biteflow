import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ActivatedRoute, Router } from '@angular/router';
import { ActivationRequests } from '../../../services/tenant-services/activation-requests';

@Component({
  selector: 'app-set-activation-password',
  standalone: true,
  templateUrl: './set-activation-password.component.html',
  styleUrl: './set-activation-password.component.css',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
  ],
})
export class SetActivationPasswordComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ActivationRequests);

  readonly submitting = signal(false);
  readonly completed = signal(false);
  readonly message = signal('');

  private userId = '';
  private token = '';

  readonly form = this.fb.group(
    {
      password: ['', [Validators.required, Validators.minLength(4)]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: [passwordsMatchValidator] }
  );

  ngOnInit(): void {
    this.userId = this.route.snapshot.queryParamMap.get('userId') ?? '';
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';

    if (!this.userId || !this.token) {
      this.message.set('Password setup link is invalid or incomplete.');
      this.form.disable();
    }
  }

  submit(): void {
    if (this.form.invalid || !this.userId || !this.token) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.message.set('');

    this.api
      .setPassword({
        userId: this.userId,
        token: this.token,
        password: this.form.controls.password.value ?? '',
      })
      .subscribe({
        next: () => {
          this.submitting.set(false);
          this.completed.set(true);
          this.form.disable();
          this.message.set('Password set successfully.');
        },
        error: (err) => {
          this.submitting.set(false);
          this.message.set(
            typeof err.error === 'string'
              ? err.error
              : 'Password setup link is invalid, expired, or already used.'
          );
        },
      });
  }

  goToLogin(): void {
    this.router.navigateByUrl('/auth/login');
  }
}

function passwordsMatchValidator(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password')?.value ?? '';
  const confirmPassword = control.get('confirmPassword')?.value ?? '';
  return password === confirmPassword ? null : { passwordsMismatch: true };
}
