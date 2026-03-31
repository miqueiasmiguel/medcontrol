import { Route } from '@angular/router';
import { authGuard } from './auth/guards/auth.guard';
import { tenantGuard } from './tenants/guards/tenant.guard';
import { doctorOnboardingGuard } from './core/guards/doctor-onboarding.guard';
import { roleGuard } from './core/guards/role.guard';
import { globalAdminGuard } from './core/guards/global-admin.guard';

export const appRoutes: Route[] = [
  {
    path: 'admin',
    canActivate: [authGuard, globalAdminGuard],
    loadChildren: () => import('./admin/admin.routes').then((m) => m.adminRoutes),
  },
  {
    path: 'privacy',
    loadChildren: () =>
      import('./privacy-policy/privacy-policy.routes').then((m) => m.privacyRoutes),
  },
  {
    path: 'auth',
    loadChildren: () => import('./auth/auth.routes').then((m) => m.authRoutes),
  },
  {
    path: 'tenants',
    canActivate: [authGuard],
    loadChildren: () => import('./tenants/tenants.routes').then((m) => m.tenantsRoutes),
  },
  {
    path: 'onboarding',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./doctors/onboarding/onboarding.component').then((m) => m.DoctorOnboardingComponent),
  },
  {
    path: '',
    canActivate: [authGuard, tenantGuard],
    canActivateChild: [doctorOnboardingGuard],
    loadComponent: () => import('./layout/shell/shell.component').then((m) => m.ShellComponent),
    children: [
      { path: '', redirectTo: 'payments', pathMatch: 'full' },
      {
        path: 'doctors',
        canActivate: [roleGuard],
        loadChildren: () => import('./doctors/doctors.routes').then((m) => m.doctorsRoutes),
      },
      {
        path: 'health-plans',
        canActivate: [roleGuard],
        loadChildren: () =>
          import('./health-plans/health-plans.routes').then((m) => m.healthPlansRoutes),
      },
      {
        path: 'procedures',
        canActivate: [roleGuard],
        loadChildren: () =>
          import('./procedures/procedures.routes').then((m) => m.proceduresRoutes),
      },
      {
        path: 'payments',
        loadChildren: () =>
          import('./payments/payments.routes').then((m) => m.paymentsRoutes),
      },
      {
        path: 'members',
        canActivate: [roleGuard],
        loadChildren: () => import('./members/members.routes').then((m) => m.membersRoutes),
      },
      {
        path: 'settings',
        loadChildren: () =>
          import('./settings/settings.routes').then((m) => m.settingsRoutes),
      },
    ],
  },
  {
    path: '**',
    redirectTo: '/auth/login',
  },
];
