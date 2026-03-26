import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { CurrentUserService } from '../data-access/current-user.service';

export const roleGuard: CanActivateFn = () => {
  const currentUser = inject(CurrentUserService);
  const router = inject(Router);

  return currentUser.getMe().pipe(
    map((user) => (user.tenantRole === 'doctor' ? router.createUrlTree(['/payments']) : true)),
    catchError(() => of(true as const)),
  );
};
