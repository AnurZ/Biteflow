export class MyConfig {
  static api_address = "https://localhost:7260/api"
  static api_base = MyConfig.api_address.replace(/\/api\/?$/, '');
  static orders_hub = `${MyConfig.api_base}/hubs/orders`;
}
