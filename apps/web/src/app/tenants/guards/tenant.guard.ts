import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of, switchMap } from 'rxjs';
import { TenantService } from '../data-access/tenant.service';

export const tenantGuard: CanActivateFn = () => {
  const tenantService = inject(TenantService);
  const router = inject(Router);

  return tenantService.getMyTenants().pipe(
    switchMap((tenants) => {
      if (tenants.length === 0) {
        return of(router.createUrlTree(['/tenants/new']));
      }

      if (tenants.length === 1) {
        return tenantService.switchTenant(tenants[0].id).pipe(
          map(() => true as const),
          catchError(() => of(router.createUrlTree(['/tenants/new']))),
        );
      }

      return of(router.createUrlTree(['/tenants/select']));
    }),
    catchError(() => of(router.createUrlTree(['/tenants/new']))),
  );
};
