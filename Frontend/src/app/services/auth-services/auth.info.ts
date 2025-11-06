// DTO to hold authentication information
export interface MyAuthInfo {
  id: string;
  restaurantId: string;
  displayName: string;
  email: string;
  roles: string[];
  isEnabled: boolean;
  isLocked: boolean;
  isLoggedIn: boolean;
}
