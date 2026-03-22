import Constants from 'expo-constants';

const API_BASE: string = (Constants.expoConfig?.extra?.apiUrl as string | undefined) ?? 'http://localhost:5000';

const headers = { 'Content-Type': 'application/json' };

async function request(path: string, options: RequestInit): Promise<Response> {
  const url = `${API_BASE}${path}`;
  console.log('[AuthService] fetch →', options.method ?? 'GET', url);
  try {
    const res = await fetch(url, {
      credentials: 'include',
      headers,
      ...options,
    });

    console.log('[AuthService] fetch ←', res.status, url);
    if (!res.ok) {
      const body = await res.json().catch(() => ({}));
      throw new Error((body as { message?: string }).message ?? `HTTP ${res.status}`);
    }

    return res;
  } catch (err) {
    console.error('[AuthService] fetch error:', (err as Error).message);
    throw err;
  }
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

  logout: async (): Promise<void> => {
    await request('/auth/logout', { method: 'POST' });
  },
};
