import {MyAuthInfo} from "./auth.info";

export interface LoginTokenDto {
  myAuthInfo: MyAuthInfo | null;
  token: string;
}
