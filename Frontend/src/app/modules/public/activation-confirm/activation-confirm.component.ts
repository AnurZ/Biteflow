import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ActivatedRoute, Router } from '@angular/router';
import { ActivationRequests } from '../../../services/tenant-services/activation-requests';

@Component({
  selector: 'app-activation-confirm',
  standalone: true,
  templateUrl: './activation-confirm.component.html',
  styleUrl: './activation-confirm.component.css',
  imports: [CommonModule, MatProgressSpinnerModule, MatIconModule, MatButtonModule],
})
export class ActivationConfirmComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(ActivationRequests);

  readonly loading = signal(true);
  readonly ok = signal(false);
  readonly message = signal('Activating…');

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token');

    if (!token) {
      this.fail('Missing activation token.');
      return;
    }

    this.api.confirm(token).subscribe({
      next: (tenantId) => {
        this.message.set('Activation complete');
        this.ok.set(true);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.ok.set(false);

        switch (err.status) {
          case 401:
            this.message.set('Activation link is invalid or has expired.');
            break;
          case 404:
            this.message.set('Activation request was not found.');
            break;
          case 409:
            this.message.set(err.error ?? 'Activation is not permitted yet.');
            break;
          default:
            this.message.set('An unexpected error occurred.');
        }
      },
    });
  }

  goHome(): void {
    this.router.navigateByUrl('/');
  }

  private fail(text: string): void {
    this.loading.set(false);
    this.ok.set(false);
    this.message.set(text);
  }
}
