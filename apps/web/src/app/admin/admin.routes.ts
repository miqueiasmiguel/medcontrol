import { Route } from '@angular/router';
import { AdminShellComponent } from './admin-shell/admin-shell.component';
import { AdminTenantsComponent } from './admin-tenants/admin-tenants.component';

export const adminRoutes: Route[] = [
  {
    path: '',
    component: AdminShellComponent,
    children: [{ path: '', component: AdminTenantsComponent }],
  },
];
