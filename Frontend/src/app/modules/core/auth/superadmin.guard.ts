import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../../../services/auth-services/auth.service';

export const SuperAdminGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const isLoggedIn = auth.isLoggedIn();
  const roles = auth.getRoles();
  const isSuperAdmin = roles.includes('superadmin');

  if (isLoggedIn && isSuperAdmin) {
    return true;
  }

  if (!isLoggedIn) {
    auth.startLogin(state.url);
    return false;
  }

  router.navigateByUrl('/public');
  return false;
};
