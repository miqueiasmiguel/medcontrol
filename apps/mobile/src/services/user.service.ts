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

export const UserService = {
  getMe: async (): Promise<UserDto> => request<UserDto>('/users/me'),
};
