import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import {MyConfig} from '../../../../my-config';
import {AuthService} from '../../../../services/auth-services/auth.service';

@Component({
  selector: 'app-logout',
  templateUrl: './logout.component.html',
  styleUrls: ['./logout.component.css'],
  standalone: false
})
export class LogoutComponent implements OnInit {
  private apiUrl = `${MyConfig.api_address}/auth/logout`;

  constructor(
    private httpClient: HttpClient,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.logout();
  }

  logout(): void {
    const refreshToken = this.authService.getLoginToken()?.refreshToken;

    // If there's no token set user to null and reroute them to home page after 3s
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
