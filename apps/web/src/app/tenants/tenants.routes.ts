import { Routes } from '@angular/router';

export const tenantsRoutes: Routes = [
  {
    path: 'select',
    loadComponent: () =>
      import('./tenant-select/tenant-select.component').then((m) => m.TenantSelectComponent),
  },
];
