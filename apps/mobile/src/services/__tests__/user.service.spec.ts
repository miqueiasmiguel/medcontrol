import { UserService } from '../user.service';

const mockFetch = jest.fn();
global.fetch = mockFetch;

beforeEach(() => {
  mockFetch.mockReset();
});

describe('UserService', () => {
  describe('getMe', () => {
    it('faz GET para /users/me com credentials include', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 200,
        json: async () => ({
          id: 'u1',
          email: 'joao@clinica.com',
          displayName: 'João Silva',
          avatarUrl: 'https://cdn.example.com/avatar.jpg',
          isEmailVerified: true,
          globalRole: 'user',
        }),
      });

      const result = await UserService.getMe();

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/users/me'),
        expect.objectContaining({ credentials: 'include' }),
      );
      expect(result.email).toBe('joao@clinica.com');
      expect(result.displayName).toBe('João Silva');
      expect(result.avatarUrl).toBe('https://cdn.example.com/avatar.jpg');
    });

    it('retorna usuário sem displayName e avatarUrl quando ausentes', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 200,
        json: async () => ({
          id: 'u2',
          email: 'maria@clinica.com',
          isEmailVerified: false,
          globalRole: 'user',
        }),
      });

      const result = await UserService.getMe();

      expect(result.displayName).toBeUndefined();
      expect(result.avatarUrl).toBeUndefined();
    });

    it('lança erro quando resposta não é 2xx', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 401,
        json: async () => ({ message: 'Unauthorized' }),
      });

      await expect(UserService.getMe()).rejects.toThrow('Unauthorized');
    });

    it('lança erro HTTP genérico quando body não tem message', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 500,
        json: async () => ({}),
      });

      await expect(UserService.getMe()).rejects.toThrow('HTTP 500');
    });
  });
});
