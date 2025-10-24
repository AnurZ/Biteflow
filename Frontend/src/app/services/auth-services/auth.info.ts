// DTO to hold authentication information
export interface MyAuthInfo {
  id:number;
  restaurantId:string; //guid
  displayName:string;
  email:string;
  isEnabled:boolean;
  isLocked:boolean;
  isLoggedIn: boolean;
}
