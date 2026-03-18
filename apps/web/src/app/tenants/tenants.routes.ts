import { Routes } from '@angular/router';

export const tenantsRoutes: Routes = [
  {
    path: 'new',
    loadComponent: () =>
      import('./tenant-new/tenant-new.component').then((m) => m.TenantNewComponent),
  },
  {
    path: 'select',
    loadComponent: () =>
      import('./tenant-select/tenant-select.component').then((m) => m.TenantSelectComponent),
  },
];
