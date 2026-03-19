import {
  ApplicationConfig,
  LOCALE_ID,
  provideBrowserGlobalErrorListeners,
  provideZoneChangeDetection,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { registerLocaleData } from '@angular/common';
import localePtBr from '@angular/common/locales/pt';
import { appRoutes } from './app.routes';
import { authInterceptor } from './auth/interceptors/auth.interceptor';
import { GOOGLE_CLIENT_ID } from './core/tokens/google-client-id.token';
import { environment } from '../environments/environment';

registerLocaleData(localePtBr);

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(appRoutes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideAnimationsAsync(),
    { provide: GOOGLE_CLIENT_ID, useValue: environment.googleClientId },
    { provide: LOCALE_ID, useValue: 'pt-BR' },
  ],
};
