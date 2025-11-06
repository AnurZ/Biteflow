import { AuthConfig } from 'angular-oauth2-oidc';

export const authConfig: AuthConfig = {
  issuer: 'https://localhost:7260',
  tokenEndpoint: 'https://localhost:7260/connect/token',
  clientId: 'biteflow-angular',
  dummyClientSecret: '',
  scope: 'openid profile email roles biteflow.api offline_access',
  requireHttps: false,
  showDebugInformation: true,
  useSilentRefresh: false,
  oidc: false
};
