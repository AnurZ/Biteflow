import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ActivationDraftDto } from '../../public/models/activation.models';
import { ActivationRequests } from '../../../services/tenant-services/activation-requests';
import { ActivationRequestRejectDialogComponent } from './activation-request-reject-dialog.component';

@Component({
  selector: 'app-activation-requests-list',
  standalone: true,
  templateUrl: './activation-requests-list.component.html',
  styleUrl: './activation-requests-list.component.css',
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatSelectModule,
    MatIconModule,
    MatInputModule,
    MatSnackBarModule,
  ],
})
export class ActivationRequestsListComponent implements OnInit {
  private readonly api = inject(ActivationRequests);
  private readonly snack = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  readonly rows = signal<ActivationDraftDto[]>([]);
  readonly loading = signal(false);

  statusFilter: number | undefined = 1;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.list(this.statusFilter).subscribe({
      next: (result) => {
        this.rows.set(result.items ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.snack.open('Failed to load activation requests', 'Close', { duration: 2500 });
      },
    });
  }

  approve(id: number): void {
    this.api.approve(id).subscribe({
      next: (link) => {
        if (navigator.clipboard) {
          navigator.clipboard.writeText(link).catch(() => undefined);
        }
        this.snack.open('Request approved. Link copied to clipboard.', 'Close', { duration: 2500 });
        this.load();
      },
      error: (err) => this.handleApproveError(err),
    });
  }

  reject(id: number): void {
    const dialogRef = this.dialog.open(ActivationRequestRejectDialogComponent, {
      width: '420px',
      disableClose: true,
    });

    dialogRef.afterClosed().subscribe((reason: string | undefined) => {
      const trimmed = reason?.trim();
      if (!trimmed) return;

      this.api.reject(id, trimmed).subscribe({
        next: () => {
          this.snack.open('Request rejected.', 'Close', { duration: 2200 });
          this.load();
        },
        error: (err) => {
          const message = this.resolveError(err, 'Reject failed');
          this.snack.open(message, 'Close', { duration: 2500 });
        },
      });
    });
  }

  statusText(status: number): string {
    switch (status) {
      case 0:
        return 'Draft';
      case 1:
        return 'Submitted';
      case 2:
        return 'Approved';
      case 3:
        return 'Activated';
      case 4:
        return 'Rejected';
      default:
        return String(status);
    }
  }

  private resolveError(err: any, fallback: string): string {
    if (!err) return fallback;
    if (typeof err.error === 'string' && err.error.trim().length > 0) {
      return err.error.trim();
    }
    if (err.error?.message) {
      return String(err.error.message);
    }
    if (err.message) {
      return String(err.message);
    }
    return fallback;
  }

  private handleApproveError(err: any): void {
    if (err?.status === 200 && typeof err.error === 'string') {
      if (navigator.clipboard) {
        navigator.clipboard.writeText(err.error).catch(() => undefined);
      }
      this.snack.open('Request approved. Link copied to clipboard.', 'Close', { duration: 2500 });
      this.load();
      return;
    }

    const message = this.resolveError(err, 'Approve failed');
    this.snack.open(message, 'Close', { duration: 2500 });
  }
}
