import { environment } from '../environments/environment';

export class MyConfig {
  static api_address = environment.apiAddress;
  static api_base = environment.apiBase;
  static orders_hub = environment.ordersHub;
  static oauth_allowed_urls = environment.oauthAllowedUrls;
  static oauth_show_debug_information = environment.oauthShowDebugInformation;
  static oauth_require_https = environment.oauthRequireHttps;
}
