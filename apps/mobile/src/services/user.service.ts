import Constants from 'expo-constants';

const API_BASE: string =
  (Constants.expoConfig?.extra?.apiUrl as string | undefined) ?? 'http://localhost:5000';

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, {
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    ...options,
  });

  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    throw new Error((body as { message?: string }).message ?? `HTTP ${res.status}`);
  }

  return res.json() as Promise<T>;
}

export interface UserDto {
  id: string;
  email: string;
  displayName?: string;
  avatarUrl?: string;
  isEmailVerified: boolean;
  globalRole: string;
  lastLoginAt?: string;
}

export interface DoctorProfileDto {
  id: string;
  tenantId: string;
  userId?: string;
  name: string;
  crm: string;
  councilState: string;
  specialty: string;
}

export interface UpdateDoctorProfileRequest {
  name: string;
  crm: string;
  councilState: string;
  specialty: string;
}

export const UserService = {
  getMe: async (): Promise<UserDto> => request<UserDto>('/users/me'),

  getDoctorProfile: async (): Promise<DoctorProfileDto | null> =>
    request<DoctorProfileDto | null>('/users/me/doctor-profile'),

  updateMyDoctorProfile: async (data: UpdateDoctorProfileRequest): Promise<DoctorProfileDto[]> =>
    request<DoctorProfileDto[]>('/users/me/doctor-profile', {
      method: 'PATCH',
      body: JSON.stringify(data),
    }),

  updateProfile: async (data: { displayName?: string }): Promise<UserDto> =>
    request<UserDto>('/users/me/profile', {
      method: 'PATCH',
      body: JSON.stringify(data),
    }),
};
