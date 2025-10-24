import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {tap} from 'rxjs/operators';
import {MyConfig} from '../../my-config';
import {AuthService} from '../../services/auth-services/auth.service';
import {LoginTokenDto} from '../../services/auth-services/login-token-dto';
import {BaseEndpointAsync} from '../../helper/base-endpoint-async';
import {jwtDecode} from 'jwt-decode';

export interface LoginRequest {
  email: string;
  password: string;
}

interface JwtPayload {
  sub: number;
  email: string;
  display_name: string;
  restaurant_id: string;
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
        const payload: JwtPayload = jwtDecode(response.accessToken);


        const loginTokenDto: LoginTokenDto = {
          accessToken: response.accessToken,
          refreshToken: response.refreshToken,
          expiresAtUtc: response.expiresAtUtc,
          myAuthInfo: {
            id: payload.sub,
            restaurantId: payload.restaurant_id,
            displayName: payload.display_name,
            email: payload.email,
            isEnabled: true,
            isLocked: false,
            isLoggedIn: true
          }
        };


        this.myAuthService.setLoggedInUser(loginTokenDto);

      })
    );
  }
}
