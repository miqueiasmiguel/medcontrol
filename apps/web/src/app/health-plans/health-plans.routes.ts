import { Route } from '@angular/router';

export const healthPlansRoutes: Route[] = [
  {
    path: '',
    loadComponent: () =>
      import('./health-plans-list/health-plans-list.component').then(
        (m) => m.HealthPlansListComponent
      ),
  },
];
