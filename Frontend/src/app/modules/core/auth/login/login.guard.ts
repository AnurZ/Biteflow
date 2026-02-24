import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../../../../services/auth-services/auth.service';

export const LoginGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  try {
    const returnUrl = state.url && state.url !== '/auth/login' ? state.url : '/auth/login';
    auth.startLogin(returnUrl);
  } catch (err) {
    // If redirect fails, navigate to a safe fallback inside the SPA
    console.error('Login redirect failed', err);
    router.navigateByUrl('/public');
  }

  return false;
};
