import { AuthConfig } from 'angular-oauth2-oidc';

export const authConfig: AuthConfig = {
  issuer: 'https://localhost:7260',
  redirectUri: `${window.location.origin}/auth/callback`,
  postLogoutRedirectUri: `${window.location.origin}/public`,
  clientId: 'biteflow-angular',
  scope: 'openid profile email roles biteflow.api offline_access',
  responseType: 'code',
  requireHttps: false,
  showDebugInformation: true,
  useSilentRefresh: false,
  oidc: true
};
