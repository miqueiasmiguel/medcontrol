import { Routes } from '@angular/router';
import { guestGuard } from './guards/guest.guard';

export const authRoutes: Routes = [
  {
    path: '',
    canActivate: [guestGuard],
    children: [
      {
        path: 'login',
        loadComponent: () =>
          import('./login/login.component').then((m) => m.LoginComponent),
      },
      {
        path: 'magic-link-sent',
        loadComponent: () =>
          import('./magic-link-sent/magic-link-sent.component').then(
            (m) => m.MagicLinkSentComponent
          ),
      },
    ],
  },
  {
    path: 'callback',
    loadComponent: () =>
      import('./google-callback/google-callback.component').then(
        (m) => m.GoogleCallbackComponent
      ),
  },
  {
    path: 'verify',
    loadComponent: () =>
      import('./magic-link-callback/magic-link-callback.component').then(
        (m) => m.MagicLinkCallbackComponent
      ),
  },
];
