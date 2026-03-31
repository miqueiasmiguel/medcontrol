import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { CurrentUserService } from '../data-access/current-user.service';

export const globalAdminGuard: CanActivateFn = () => {
  const currentUser = inject(CurrentUserService);
  const router = inject(Router);

  return currentUser.getMe().pipe(
    map((user) =>
      user.globalRole === 'Admin' ? true : router.createUrlTree(['/']),
    ),
    catchError(() => of(router.createUrlTree(['/']))),
  );
};
