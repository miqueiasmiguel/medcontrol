import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class SessionService {
  isAuthenticated(): boolean {
    return document.cookie.split(';').some((c) => c.trim().startsWith('mmc_session='));
  }
}
