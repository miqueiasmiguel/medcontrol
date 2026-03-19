import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface UserDto {
  id: string;
  email: string;
  displayName: string | null;
  avatarUrl: string | null;
  isEmailVerified: boolean;
  globalRole: string;
  lastLoginAt: string | null;
}

export interface UpdateProfileRequest {
  displayName: string | null;
}

@Injectable({ providedIn: 'root' })
export class SettingsService {
  private readonly http = inject(HttpClient);

  getMe(): Observable<UserDto> {
    return this.http.get<UserDto>('/api/users/me', { withCredentials: true });
  }

  updateProfile(req: UpdateProfileRequest): Observable<UserDto> {
    return this.http.patch<UserDto>('/api/users/me/profile', req, { withCredentials: true });
  }
}
