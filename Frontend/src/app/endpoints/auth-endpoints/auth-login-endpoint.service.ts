import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {tap} from 'rxjs/operators';
import {MyConfig} from '../../my-config';
import {AuthService} from '../../services/auth-services/auth.service';
import {LoginTokenDto} from '../../services/auth-services/login-token-dto';
import {BaseEndpointAsync} from '../../helper/base-endpoint-async';


export interface LoginRequest {
  email: string;
  password: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthLoginEndpointService implements BaseEndpointAsync<LoginRequest, LoginTokenDto> {
  private apiUrl = `${MyConfig.api_address}/auth/login`;

  constructor(private httpClient: HttpClient, private myAuthService: AuthService) {
  }

  handleAsync(request: LoginRequest) {
    return this.httpClient.post<LoginTokenDto>(`${this.apiUrl}`, request).pipe(
      tap((response) => {
        // Use MyAuthService to store login token and auth info
        this.myAuthService.setLoggedInUser({
          token: response.token,
          myAuthInfo: response.myAuthInfo
        });
      })
    );
  }
}
