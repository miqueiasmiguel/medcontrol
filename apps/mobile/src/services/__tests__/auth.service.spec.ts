import { AuthService } from '../auth.service';

const mockFetch = jest.fn();
global.fetch = mockFetch;

beforeEach(() => {
  mockFetch.mockReset();
});

describe('AuthService', () => {
  describe('sendMagicLink', () => {
    it('faz POST para /auth/magic-link/send com o email', async () => {
      mockFetch.mockResolvedValueOnce({ ok: true, status: 204 });

      await AuthService.sendMagicLink('user@test.com');

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/auth/magic-link/send'),
        expect.objectContaining({
          method: 'POST',
          credentials: 'include',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ email: 'user@test.com' }),
        }),
      );
    });

    it('lança erro quando status não é 2xx', async () => {
      mockFetch.mockResolvedValueOnce({ ok: false, status: 400, json: async () => ({ message: 'Bad Request' }) });

      await expect(AuthService.sendMagicLink('invalid')).rejects.toThrow();
    });
  });

  describe('verifyMagicLink', () => {
    it('faz POST para /auth/magic-link/verify com o token', async () => {
      mockFetch.mockResolvedValueOnce({ ok: true, status: 204 });

      await AuthService.verifyMagicLink('my-token-123');

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/auth/magic-link/verify'),
        expect.objectContaining({
          method: 'POST',
          credentials: 'include',
          body: JSON.stringify({ token: 'my-token-123' }),
        }),
      );
    });

    it('lança erro quando token é inválido', async () => {
      mockFetch.mockResolvedValueOnce({ ok: false, status: 401, json: async () => ({ message: 'Unauthorized' }) });

      await expect(AuthService.verifyMagicLink('bad-token')).rejects.toThrow();
    });
  });

  describe('loginWithGoogle', () => {
    it('faz POST para /auth/google/callback com code e redirectUri', async () => {
      mockFetch.mockResolvedValueOnce({ ok: true, status: 204 });

      await AuthService.loginWithGoogle('auth-code', 'medcontrol://google-callback');

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/auth/google/callback'),
        expect.objectContaining({
          method: 'POST',
          credentials: 'include',
          body: JSON.stringify({ code: 'auth-code', redirectUri: 'medcontrol://google-callback' }),
        }),
      );
    });

    it('lança erro quando código do Google é inválido', async () => {
      mockFetch.mockResolvedValueOnce({ ok: false, status: 401, json: async () => ({ message: 'Unauthorized' }) });

      await expect(AuthService.loginWithGoogle('bad-code', 'medcontrol://google-callback')).rejects.toThrow();
    });
  });

  describe('verifyGoogleIdToken', () => {
    it('faz POST para /auth/google/verify com o idToken', async () => {
      mockFetch.mockResolvedValueOnce({ ok: true, status: 204 });

      await AuthService.verifyGoogleIdToken('my-id-token');

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/auth/google/verify'),
        expect.objectContaining({
          method: 'POST',
          credentials: 'include',
          body: JSON.stringify({ idToken: 'my-id-token' }),
        }),
      );
    });

    it('lança erro quando id_token é inválido', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 401,
        json: async () => ({ message: 'Unauthorized' }),
      });

      await expect(AuthService.verifyGoogleIdToken('bad-token')).rejects.toThrow();
    });
  });

  describe('logout', () => {
    it('faz POST para /auth/logout', async () => {
      mockFetch.mockResolvedValueOnce({ ok: true, status: 204 });

      await AuthService.logout();

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/auth/logout'),
        expect.objectContaining({
          method: 'POST',
          credentials: 'include',
        }),
      );
    });
  });
});
