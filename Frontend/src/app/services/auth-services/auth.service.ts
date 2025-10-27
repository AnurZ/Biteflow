import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MyAuthInfo } from './auth.info';
import { LoginTokenDto } from './login-token-dto';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  constructor(private httpClient: HttpClient) {}

  getMyAuthInfo(): MyAuthInfo | null {
    return this.getLoginToken()?.myAuthInfo ?? null;
  }

  isLoggedIn(): boolean {
    const authInfo = this.getMyAuthInfo();
    return authInfo != null && authInfo.isLoggedIn && authInfo.isEnabled;
  }

  isLocked(): boolean {
    return this.getMyAuthInfo()?.isLocked ?? false;
  }

  getDisplayName(): string {
    return this.getMyAuthInfo()?.displayName ?? '';
  }

  setLoggedInUser(tokenDto: LoginTokenDto | null) {
    if (tokenDto == null) {
      window.localStorage.removeItem('my-auth-token');
    } else {
      console.log("Saving tokenDto:", tokenDto);
      window.localStorage.setItem('my-auth-token', JSON.stringify(tokenDto));

    }
  }

  getLoginToken(): LoginTokenDto | null {
    const tokenString = window.localStorage.getItem('my-auth-token') ?? '';
    if (!tokenString) return null;
    try {
      return JSON.parse(tokenString);
    } catch {
      console.log("Could not parse login token");
      return null;
    }
  }

  logout(): void {
    this.setLoggedInUser(null);
  }
}
