import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  if (req.url.startsWith('/api')) {
    return next(req.clone({ withCredentials: true })).pipe(
      catchError((error: unknown) => {
        if (
          error instanceof HttpErrorResponse &&
          error.status === 401 &&
          !req.url.startsWith('/api/auth')
        ) {
          router.navigate(['/auth/login']);
        }
        return throwError(() => error);
      }),
    );
  }
  return next(req);
};
