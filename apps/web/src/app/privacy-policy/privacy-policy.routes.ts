import { Route } from '@angular/router';

export const privacyRoutes: Route[] = [
  {
    path: '',
    loadComponent: () =>
      import('./privacy-policy.component').then((m) => m.PrivacyPolicyComponent),
  },
];
