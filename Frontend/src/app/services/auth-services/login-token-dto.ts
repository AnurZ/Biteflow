import {MyAuthInfo} from "./auth.info";

export interface LoginTokenDto {
  accessToken: string;
  refreshToken: string;
  expiresAtUtc: string;
  myAuthInfo: MyAuthInfo | null;
}
