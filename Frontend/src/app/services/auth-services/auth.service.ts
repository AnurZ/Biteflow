import { Injectable, NgZone } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { OAuthService } from 'angular-oauth2-oidc';
import { from, lastValueFrom, map, Observable } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import { authConfig } from '../../auth-config';
import { MyAuthInfo } from './auth.info';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private authInfo: MyAuthInfo | null = null;
  private discoveryDocumentPromise: Promise<void>;

  constructor(
    private readonly oauthService: OAuthService,
    private readonly zone: NgZone,
    private readonly http: HttpClient
  ) {
    this.oauthService.configure(authConfig);
    this.oauthService.setStorage(localStorage);

    this.discoveryDocumentPromise = this.oauthService
      .loadDiscoveryDocument()
      .then(() => void 0)
      .catch((error) => {
        console.warn('Failed to load OAuth discovery document.', error);
      });

    this.oauthService.tryLogin().catch(() => void 0);

    this.oauthService.events.subscribe(() => {
      this.zone.run(() => this.updateAuthInfoFromToken());
    });
    this.updateAuthInfoFromToken();
  }

  login(username: string, password: string): Observable<MyAuthInfo> {
    return from(this.ensureDiscoveryDocumentLoaded()
      .then(() => this.oauthService.fetchTokenUsingPasswordFlow(username, password))
      .then(() => this.fetchUserInfo())
      .then(profile => this.zone.run(() => this.updateAuthInfoFromToken(profile)))
    ).pipe(
      map(() => {
        const info = this.getMyAuthInfo();
        if (!info) {
          throw new Error('Authentication failed.');
        }
        return info;
      })
    );
  }

  logout(): Observable<void> {
    return from(this.ensureDiscoveryDocumentLoaded().then(() => {
      this.oauthService.logOut(true);
      this.zone.run(() => this.updateAuthInfoFromToken());
    })).pipe(map(() => void 0));
  }

  isLoggedIn(): boolean {
    return this.oauthService.hasValidAccessToken();
  }

  getAccessToken(): string | null {
    return this.oauthService.getAccessToken() || null;
  }

  getMyAuthInfo(): MyAuthInfo | null {
    return this.authInfo;
  }

  isLocked(): boolean {
    return this.authInfo?.isLocked ?? false;
  }

  getRoles(): string[] {
    return this.authInfo?.roles ?? [];
  }

  getDisplayName(): string {
    return this.authInfo?.displayName ?? '';
  }

  private updateAuthInfoFromToken(userInfo?: Record<string, any>): void {
    const accessToken = this.oauthService.getAccessToken();
    if (!accessToken) {
      this.authInfo = null;
      return;
    }

    try {
      const payload: any = jwtDecode(accessToken);
      const info = {
        id: String(payload.sub ?? ''),
        restaurantId: String(payload['restaurant_id'] ?? payload['restaurantid'] ?? ''),
        displayName: '',
        email: String(
          payload['email'] ??
          payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ??
          userInfo?.['email'] ??
          ''
        ),
        roles: this.extractRoles(payload),
        isEnabled: payload['is_enabled'] !== false,
        isLocked: payload['is_locked'] === true,
        isLoggedIn: true
      };

      info.displayName = String(
        userInfo?.['display_name'] ??
        userInfo?.['name'] ??
        userInfo?.['preferred_username'] ??
        payload['display_name'] ??
        payload['name'] ??
        payload['preferred_username'] ??
        ''
      );

      this.authInfo = info;

      if (!this.authInfo.displayName) {
        console.warn('Display name missing in token payload and userinfo', payload, userInfo);
      }
    } catch (error) {
      console.warn('Failed to decode access token.', error);
      this.authInfo = null;
    }
  }

  private extractRoles(payload: Record<string, any>): string[] {
    const roleClaim = payload['role'] ?? payload['roles'] ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    if (!roleClaim) return [];

    if (Array.isArray(roleClaim)) {
      return roleClaim.map((r) => String(r));
    }

    return String(roleClaim)
      .split(',')
      .map((r) => r.trim())
      .filter((r) => r.length > 0);
  }

  private async ensureDiscoveryDocumentLoaded(): Promise<void> {
    await this.discoveryDocumentPromise.catch(() => void 0);
  }

  private async fetchUserInfo(): Promise<Record<string, any> | undefined> {
    const token = this.oauthService.getAccessToken();
    if (!token) return undefined;

    const issuer = this.oauthService.issuer ?? authConfig.issuer;
    if (!issuer) return undefined;

    try {
      const url = `${issuer.replace(/\/+$/, '')}/connect/userinfo`;
      const result = await lastValueFrom(
        this.http.get<Record<string, any>>(url, {
          headers: { Authorization: `Bearer ${token}` }
        })
      );
      return result;
    } catch (error) {
      console.warn('error loading user info', error);
      return undefined;
    }
  }
}
