import { act, renderHook } from '@testing-library/react-native';
import { UserService } from '../../services/user.service';
import { useCurrentUser } from '../useCurrentUser';

jest.mock('../../services/user.service');

const mockUserService = UserService as jest.Mocked<typeof UserService>;

const fakeUser = {
  id: 'u1',
  email: 'joao@clinica.com',
  displayName: 'João Silva',
  isEmailVerified: true,
  globalRole: 'user',
};

beforeEach(() => {
  jest.clearAllMocks();
});

describe('useCurrentUser', () => {
  it('começa com loading true e user null', () => {
    mockUserService.getMe.mockResolvedValue(fakeUser);
    const { result } = renderHook(() => useCurrentUser());

    expect(result.current.loading).toBe(true);
    expect(result.current.user).toBeNull();
    expect(result.current.error).toBeNull();
  });

  it('carrega o usuário ao montar', async () => {
    mockUserService.getMe.mockResolvedValue(fakeUser);
    const { result } = renderHook(() => useCurrentUser());

    await act(() => Promise.resolve());

    expect(result.current.loading).toBe(false);
    expect(result.current.user).toEqual(fakeUser);
    expect(result.current.error).toBeNull();
  });

  it('define error quando getMe falha', async () => {
    mockUserService.getMe.mockRejectedValue(new Error('Unauthorized'));
    const { result } = renderHook(() => useCurrentUser());

    await act(() => Promise.resolve());

    expect(result.current.loading).toBe(false);
    expect(result.current.user).toBeNull();
    expect(result.current.error).toBe('Unauthorized');
  });

  it('usa mensagem genérica quando erro não é instância de Error', async () => {
    mockUserService.getMe.mockRejectedValue('network failure');
    const { result } = renderHook(() => useCurrentUser());

    await act(() => Promise.resolve());

    expect(result.current.error).toBe('Erro ao carregar usuário');
  });
});
