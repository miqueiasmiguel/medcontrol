import { Route } from '@angular/router';

export const proceduresRoutes: Route[] = [
  {
    path: '',
    loadComponent: () =>
      import('./procedures-list/procedures-list.component').then(
        (m) => m.ProceduresListComponent
      ),
  },
];
