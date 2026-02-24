import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../../../services/auth-services/auth.service';

export const WaiterGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isLoggedIn()) {
    auth.startLogin(state.url);
    return false;
  }

  if (auth.hasWaiterAccess() || auth.hasRole('staff') || auth.hasRole('admin')) {
    return true;
  }

  router.navigateByUrl('/public');
  return false;
};
