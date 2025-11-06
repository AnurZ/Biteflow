import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { AuthService } from '../../../../services/auth-services/auth.service';
import { LogoutConfirmDialogComponent } from './logout-confirm-dialog.component';

@Component({
  selector: 'app-logout',
  templateUrl: './logout.component.html',
  styleUrls: ['./logout.component.css'],
  standalone: false
})
export class LogoutComponent {
  confirmed = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.openConfirmDialog();
  }

  openConfirmDialog(): void {
    const dialogRef = this.dialog.open(LogoutConfirmDialogComponent, {
      width: '360px',
      disableClose: true
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.confirmed = true;
        this.logout();
      } else {
        this.router.navigate(['/public']);
      }
    });
  }

  logout(): void {
    this.authService.logout().subscribe({
      next: () => this.handleLogoutCompletion(),
      error: (error) => {
        console.error('Error during logout:', error);
        this.handleLogoutCompletion();
      }
    });
  }

  private handleLogoutCompletion(): void {
    setTimeout(() => {
      this.router.navigate(['/public']);
    }, 3000);
  }
}
