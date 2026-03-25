import { UserService } from '../user.service';
import { onUnauthorized } from '../../lib/unauthorizedEmitter';

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

    it('emite evento unauthorized ao receber 401', async () => {
      const listener = jest.fn();
      const unsubscribe = onUnauthorized(listener);

      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 401,
        json: async () => ({ message: 'Unauthorized' }),
      });

      await expect(UserService.getMe()).rejects.toThrow();
      expect(listener).toHaveBeenCalledTimes(1);

      unsubscribe();
    });

    it('não emite evento unauthorized para outros erros http', async () => {
      const listener = jest.fn();
      const unsubscribe = onUnauthorized(listener);

      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 500,
        json: async () => ({}),
      });

      await expect(UserService.getMe()).rejects.toThrow();
      expect(listener).not.toHaveBeenCalled();

      unsubscribe();
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

  describe('getDoctorProfile', () => {
    it('faz GET para /users/me/doctor-profile', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 200,
        json: async () => ({
          id: 'd1',
          tenantId: 't1',
          userId: 'u1',
          name: 'Dr. João',
          crm: '123456',
          councilState: 'SP',
          specialty: 'Cardiologia',
        }),
      });

      const result = await UserService.getDoctorProfile();

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/users/me/doctor-profile'),
        expect.objectContaining({ credentials: 'include' }),
      );
      expect(result?.name).toBe('Dr. João');
      expect(result?.crm).toBe('123456');
    });

    it('lança erro quando resposta não é 2xx', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 401,
        json: async () => ({ message: 'Unauthorized' }),
      });

      await expect(UserService.getDoctorProfile()).rejects.toThrow('Unauthorized');
    });
  });

  describe('updateMyDoctorProfile', () => {
    it('faz PATCH para /users/me/doctor-profile com os dados', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 200,
        json: async () => [
          { id: 'd1', tenantId: 't1', name: 'Dr. João Atualizado', crm: '654321', councilState: 'RJ', specialty: 'Neurologia' },
        ],
      });

      const result = await UserService.updateMyDoctorProfile({
        name: 'Dr. João Atualizado',
        crm: '654321',
        councilState: 'RJ',
        specialty: 'Neurologia',
      });

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/users/me/doctor-profile'),
        expect.objectContaining({
          method: 'PATCH',
          body: JSON.stringify({ name: 'Dr. João Atualizado', crm: '654321', councilState: 'RJ', specialty: 'Neurologia' }),
        }),
      );
      expect(result).toHaveLength(1);
      expect(result[0].name).toBe('Dr. João Atualizado');
    });
  });

  describe('updateProfile', () => {
    it('faz PATCH para /users/me/profile com displayName', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 200,
        json: async () => ({ id: 'u1', email: 'joao@clinica.com', displayName: 'Novo Nome', isEmailVerified: true, globalRole: 'user' }),
      });

      const result = await UserService.updateProfile({ displayName: 'Novo Nome' });

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/users/me/profile'),
        expect.objectContaining({
          method: 'PATCH',
          body: JSON.stringify({ displayName: 'Novo Nome' }),
        }),
      );
      expect(result.displayName).toBe('Novo Nome');
    });
  });
});
