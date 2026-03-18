import { Route } from '@angular/router';

export const doctorsRoutes: Route[] = [
  {
    path: '',
    loadComponent: () =>
      import('./doctors-list/doctors-list.component').then((m) => m.DoctorsListComponent),
  },
];
