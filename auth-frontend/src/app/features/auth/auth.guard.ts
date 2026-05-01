import { inject } from '@angular/core';
import { CanActivateFn, Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from './auth.service';
import { map } from 'rxjs/operators';

export const authGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const token = auth.getAccessToken();

  if (!token) {
    return router.createUrlTree(['/auth/login']);
  }

  if (!auth.isTokenExpired(token)) {
    return checkRole();
  }

  return auth.refresh().pipe(
    map((success) => {
      if (!success) {
        return router.createUrlTree(['/auth/login']);
      }
      return checkRole();
    }),
  );

  function checkRole() {
    const userRole = auth.getUserRole();
    const allowedRoles = route.data?.['roles'] as string[] | undefined;

    if (!allowedRoles || allowedRoles.length === 0) {
      return true;
    }

    if (userRole && allowedRoles.includes(userRole)) {
      return true;
    }

    return router.createUrlTree(['/auth/unauthorized']);
  }
};
