import { Route } from '@angular/router';

export const membersRoutes: Route[] = [
  {
    path: '',
    loadComponent: () => import('./members.component').then((m) => m.MembersComponent),
  },
];
