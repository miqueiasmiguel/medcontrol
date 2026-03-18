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
    loadComponent: () => import('./app').then((m) => m.App),
  },
  {
    path: '**',
    redirectTo: '/auth/login',
  },
];
