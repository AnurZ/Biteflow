import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';
import { Observable, of } from 'rxjs';
import { MyConfig } from '../../my-config';
import { AuthService } from '../../services/auth-services/auth.service';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';

@Injectable({
  providedIn: 'root'
})
export class AuthLogoutEndpointService implements BaseEndpointAsync<void, void> {
  private apiUrl = `${MyConfig.api_address}/auth/logout`;

  constructor(
    private httpClient: HttpClient,
    private authService: AuthService
  ) {}

  handleAsync(_: void): Observable<void> {
    const refreshToken = this.authService.getLoginToken()?.refreshToken;

    if (!refreshToken) {
      console.warn('No refresh token found during logout.');
      this.authService.setLoggedInUser(null);
      return of(void 0);
    }

    return this.httpClient.post<void>(
      this.apiUrl,
      { refreshToken }
    ).pipe(
      tap({
        next: () => {
          this.authService.setLoggedInUser(null);
          console.log('LogoutComponent successful');
        },
        error: (err) => {
          console.error('Error during logout:', err);
          this.authService.setLoggedInUser(null);
        }
      })
    );
  }
}
