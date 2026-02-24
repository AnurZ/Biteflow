import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AuthService } from '../../../services/auth-services/auth.service';

export const LoggedInGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);

  if (auth.isLoggedIn()) {
    return true;
  }

  auth.startLogin(state.url);
  return false;
};
