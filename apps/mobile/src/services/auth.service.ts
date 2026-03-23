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
    await request('/auth/magic-link/send', {
      method: 'POST',
      body: JSON.stringify({ email }),
    });
  },

  verifyMagicLink: async (token: string): Promise<void> => {
    await request('/auth/magic-link/verify', {
      method: 'POST',
      body: JSON.stringify({ token }),
    });
  },

  loginWithGoogle: async (code: string, redirectUri: string): Promise<void> => {
    await request('/auth/google/callback', {
      method: 'POST',
      body: JSON.stringify({ code, redirectUri }),
    });
  },

  verifyGoogleIdToken: async (idToken: string): Promise<void> => {
    await request('/auth/google/verify', {
      method: 'POST',
      body: JSON.stringify({ idToken }),
    });
  },

  logout: async (): Promise<void> => {
    await request('/auth/logout', { method: 'POST' });
  },
};
