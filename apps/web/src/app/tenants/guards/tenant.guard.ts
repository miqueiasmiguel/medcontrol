import { HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of, switchMap } from 'rxjs';
import { TenantService } from '../data-access/tenant.service';
import { CurrentUserService } from '../../core/data-access/current-user.service';

export const tenantGuard: CanActivateFn = () => {
  const tenantService = inject(TenantService);
  const currentUser = inject(CurrentUserService);
  const router = inject(Router);

  return currentUser.getMe().pipe(
    switchMap((user) =>
      tenantService.getMyTenants().pipe(
        switchMap((tenants) => {
          if (tenants.length === 0) {
            return of(
              router.createUrlTree(
                user.globalRole === 'Admin' ? ['/admin'] : ['/tenants/new'],
              ),
            );
          }

          if (tenants.length === 1) {
            return tenantService.switchTenant(tenants[0].id).pipe(
              map(() => true as const),
              catchError(() => of(router.createUrlTree(['/tenants/new']))),
            );
          }

          if (user.tenantRole) {
            return of(true as const);
          }
          return of(router.createUrlTree(['/tenants/select']));
        }),
      ),
    ),
    catchError((err: unknown) => {
      if (err instanceof HttpErrorResponse && err.status === 401) {
        return of(router.createUrlTree(['/auth/login']));
      }
      return of(router.createUrlTree(['/tenants/new']));
    }),
  );
};
