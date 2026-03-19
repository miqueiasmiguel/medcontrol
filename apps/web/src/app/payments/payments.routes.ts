import { Route } from '@angular/router';

export const paymentsRoutes: Route[] = [
  {
    path: '',
    loadComponent: () =>
      import('./payments-list/payments-list.component').then((m) => m.PaymentsListComponent),
  },
];
