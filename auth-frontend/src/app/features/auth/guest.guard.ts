import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const guestGuard: CanMatchFn = async () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const token = auth.getAccessToken();

  // logged in → block access to /auth entirely
  if (token && !auth.isTokenExpired(token)) {
    return router.createUrlTree([auth.getDashboardRoute()]);
  }

  return true;
};
