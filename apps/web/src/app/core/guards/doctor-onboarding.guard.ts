import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of, switchMap } from 'rxjs';
import { SettingsService } from '../../settings/data-access/settings.service';
import { DoctorService } from '../../doctors/data-access/doctor.service';
import { WINDOW } from '../tokens/window.token';

export const doctorOnboardingGuard: CanActivateFn = () => {
  const win = inject(WINDOW);
  const router = inject(Router);
  const settingsService = inject(SettingsService);
  const doctorService = inject(DoctorService);

  if (win.sessionStorage.getItem('mmc_onboarding_skip')) {
    return true;
  }

  return settingsService.getMe().pipe(
    switchMap((user) => {
      if (user.tenantRole !== 'doctor') {
        return of(true as const);
      }
      return doctorService.getMyDoctorProfile().pipe(
        map((profile) =>
          profile ? (true as const) : router.createUrlTree(['/onboarding']),
        ),
        catchError(() => of(true as const)),
      );
    }),
    catchError(() => of(true as const)),
  );
};
