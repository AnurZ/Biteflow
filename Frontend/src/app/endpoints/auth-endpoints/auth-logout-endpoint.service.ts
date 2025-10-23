import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable} from 'rxjs';
import {MyConfig} from '../../my-config';
import {AuthService} from '../../services/auth-services/auth.service';
import {BaseEndpointAsync} from '../../helper/base-endpoint-async';

@Injectable({
  providedIn: 'root'
})
export class AuthLogoutEndpointService implements BaseEndpointAsync<void, void> {
  private apiUrl = `${MyConfig.api_address}/auth/logout`;

  constructor(private httpClient: HttpClient, private authService: AuthService) {
  }

  handleAsync() {
    return new Observable<void>((observer) => {
      this.httpClient.post<void>(this.apiUrl, {}).subscribe({
        next: () => {

          this.authService.setLoggedInUser(null); // Removes token from localStorage
          observer.next();
          observer.complete();
        },
        error: (error) => {
          console.error('Error during logout:', error);
          observer.error(error);
          this.authService.setLoggedInUser(null); // Removes token from localStorage
        }
      });
    });
  }
}
