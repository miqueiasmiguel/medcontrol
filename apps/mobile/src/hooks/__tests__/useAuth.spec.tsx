import { act, renderHook } from '@testing-library/react-native';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { useAuth } from '../useAuth';
import { AuthService } from '../../services/auth.service';

jest.mock('@react-native-async-storage/async-storage', () =>
  require('@react-native-async-storage/async-storage/jest/async-storage-mock'),
);
jest.mock('../../services/auth.service');

const mockAuthService = AuthService as jest.Mocked<typeof AuthService>;

beforeEach(async () => {
  jest.clearAllMocks();
  await AsyncStorage.clear();
});

describe('useAuth', () => {
  it('começa com isAuthenticated false e isLoading false após a inicialização', async () => {
    const { result } = renderHook(() => useAuth());

    await act(async () => {});

    expect(result.current.isAuthenticated).toBe(false);
    expect(result.current.isLoading).toBe(false);
  });

  it('retorna isAuthenticated true quando há sessão no AsyncStorage', async () => {
    await AsyncStorage.setItem('mmc_session', '1');

    const { result } = renderHook(() => useAuth());
    await act(async () => {});

    expect(result.current.isAuthenticated).toBe(true);
  });

  it('setSession armazena sessão e atualiza estado para autenticado', async () => {
    const { result } = renderHook(() => useAuth());
    await act(async () => {});

    await act(async () => {
      await result.current.setSession(true);
    });

    expect(result.current.isAuthenticated).toBe(true);
    expect(await AsyncStorage.getItem('mmc_session')).toBe('1');
  });

  it('logout chama AuthService.logout e limpa estado e AsyncStorage', async () => {
    mockAuthService.logout.mockResolvedValueOnce(undefined);
    await AsyncStorage.setItem('mmc_session', '1');

    const { result } = renderHook(() => useAuth());
    await act(async () => {});

    await act(async () => {
      await result.current.logout();
    });

    expect(mockAuthService.logout).toHaveBeenCalledTimes(1);
    expect(result.current.isAuthenticated).toBe(false);
    expect(await AsyncStorage.getItem('mmc_session')).toBeNull();
  });
});
