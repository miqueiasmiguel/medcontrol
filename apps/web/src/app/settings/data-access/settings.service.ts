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
  tenantRole?: string | null;
}

export interface UpdateProfileRequest {
  displayName: string | null;
}

export interface UpdateMyDoctorProfileRequest {
  name: string;
  crm: string;
  councilState: string;
  specialty: string;
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

  updateMyDoctorProfile(req: UpdateMyDoctorProfileRequest): Observable<unknown> {
    return this.http.patch('/api/users/me/doctor-profile', req, { withCredentials: true });
  }
}
