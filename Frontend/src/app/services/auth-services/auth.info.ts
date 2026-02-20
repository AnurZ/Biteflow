// DTO to hold authentication information
export interface MyAuthInfo {
  id: string;
  restaurantId: string;
  tenantName: string;
  displayName: string;
  position: string;
  email: string;
  roles: string[];
  isEnabled: boolean;
  isLocked: boolean;
  isLoggedIn: boolean;
}
