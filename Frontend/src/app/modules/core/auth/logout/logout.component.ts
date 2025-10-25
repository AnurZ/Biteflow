import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MyConfig } from '../../../../my-config';
import { AuthService } from '../../../../services/auth-services/auth.service';
import { LogoutConfirmDialogComponent } from './logout-confirm-dialog.component';

@Component({
  selector: 'app-logout',
  templateUrl: './logout.component.html',
  styleUrls: ['./logout.component.css'],
  standalone: false
})
export class LogoutComponent {
  private apiUrl = `${MyConfig.api_address}/auth/logout`;
  confirmed = false;

  constructor(
    private httpClient: HttpClient,
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
    const refreshToken = this.authService.getLoginToken()?.refreshToken;

    if (!refreshToken) {
      this.handleLogoutSuccessOrError();
      return;
    }

    this.httpClient.post<void>(this.apiUrl, { refreshToken }).subscribe({
      next: () => this.handleLogoutSuccessOrError(),
      error: (error) => {
        console.error('Error during logout:', error);
        this.handleLogoutSuccessOrError();
      }
    });
  }

  private handleLogoutSuccessOrError(): void {
    this.authService.setLoggedInUser(null);
    setTimeout(() => {
      this.router.navigate(['/public']);
    }, 3000);
  }
}
