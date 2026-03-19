import { Route } from '@angular/router';
import { authGuard } from './auth/guards/auth.guard';
import { tenantGuard } from './tenants/guards/tenant.guard';

export const appRoutes: Route[] = [
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
    path: '',
    canActivate: [authGuard, tenantGuard],
    loadComponent: () => import('./layout/shell/shell.component').then((m) => m.ShellComponent),
    children: [
      { path: '', redirectTo: 'doctors', pathMatch: 'full' },
      {
        path: 'doctors',
        loadChildren: () => import('./doctors/doctors.routes').then((m) => m.doctorsRoutes),
      },
      {
        path: 'health-plans',
        loadChildren: () =>
          import('./health-plans/health-plans.routes').then((m) => m.healthPlansRoutes),
      },
      {
        path: 'procedures',
        loadChildren: () =>
          import('./procedures/procedures.routes').then((m) => m.proceduresRoutes),
      },
      {
        path: 'payments',
        loadChildren: () =>
          import('./payments/payments.routes').then((m) => m.paymentsRoutes),
      },
    ],
  },
  {
    path: '**',
    redirectTo: '/auth/login',
  },
];
