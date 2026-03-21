import Constants from 'expo-constants';

const API_BASE: string = (Constants.expoConfig?.extra?.apiUrl as string | undefined) ?? 'http://localhost:5000';

const headers = { 'Content-Type': 'application/json' };

async function request(path: string, options: RequestInit): Promise<Response> {
  const res = await fetch(`${API_BASE}${path}`, {
    credentials: 'include',
    headers,
    ...options,
  });

  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    throw new Error((body as { message?: string }).message ?? `HTTP ${res.status}`);
  }

  return res;
}

export const AuthService = {
  sendMagicLink: async (email: string): Promise<void> => {
    await request('/api/auth/magic-link/send', {
      method: 'POST',
      body: JSON.stringify({ email }),
    });
  },

  verifyMagicLink: async (token: string): Promise<void> => {
    await request('/api/auth/magic-link/verify', {
      method: 'POST',
      body: JSON.stringify({ token }),
    });
  },

  loginWithGoogle: async (
    code: string,
    redirectUri: string,
  ): Promise<{ accessToken: string; refreshToken: string; expiresIn: number; tokenType: string }> => {
    const res = await request('/api/auth/google/callback', {
      method: 'POST',
      body: JSON.stringify({ code, redirectUri }),
    });
    return res.json() as Promise<{ accessToken: string; refreshToken: string; expiresIn: number; tokenType: string }>;
  },

  logout: async (): Promise<void> => {
    await request('/api/auth/logout', { method: 'POST' });
  },
};
