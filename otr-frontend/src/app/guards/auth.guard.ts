import { inject } from '@angular/core';
import { CanActivateFn, GuardResult, Router } from '@angular/router';

import { AuthService } from '@/services/auth.service';

/** Lets logged-in users through and redirects everyone else to the login page. */
export const authGuard: CanActivateFn = (): GuardResult => {
  const authService: AuthService = inject(AuthService);
  const router: Router = inject(Router);

  if (authService.isLoggedIn()) {
    return true;
  }

  return router.createUrlTree(['/login']);
};
