import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  sendMagicLink(email: string) {
    return this.http.post('/api/auth/magic-link/send', { email }, { withCredentials: true });
  }

  verifyMagicLink(token: string) {
    return this.http.post('/api/auth/magic-link/verify', { token }, { withCredentials: true });
  }

  loginWithGoogle(code: string, redirectUri: string) {
    return this.http.post('/api/auth/google/callback', { code, redirectUri }, { withCredentials: true });
  }

  logout() {
    return this.http.post('/api/auth/logout', null, { withCredentials: true });
  }
}
