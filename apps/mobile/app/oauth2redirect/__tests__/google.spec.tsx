import React from 'react';
import { render, screen, waitFor } from '@testing-library/react-native';
import { PaperProvider } from 'react-native-paper';

jest.mock('../../../src/services/auth.service');
jest.mock('@react-native-async-storage/async-storage', () =>
  require('@react-native-async-storage/async-storage/jest/async-storage-mock'),
);

const mockReplace = jest.fn();
const mockDispatch = jest.fn();
let mockCode: string | undefined = 'auth-code-from-google-123';

jest.mock('expo-router', () => ({
  useRouter: () => ({ replace: mockReplace }),
  useLocalSearchParams: () => ({ code: mockCode }),
  useRootNavigation: () => ({ dispatch: mockDispatch }),
}));

jest.mock('@react-navigation/native', () => ({
  CommonActions: {
    reset: jest.fn((state: unknown) => ({ type: 'RESET', state })),
  },
}));

jest.mock('../../../src/hooks/useAuth', () => ({
  useAuth: () => ({ setSession: jest.fn().mockResolvedValue(undefined) }),
}));

jest.mock('expo-constants', () => ({
  default: {
    expoConfig: {
      extra: {
        googleAndroidClientId:
          '545148539649-m12d16iqkq3vvjorm7aqjftkohlmuibo.apps.googleusercontent.com',
      },
    },
  },
}));

const mockFetch = jest.fn();
global.fetch = mockFetch;

import AsyncStorage from '@react-native-async-storage/async-storage';
import { AuthService } from '../../../src/services/auth.service';
import GoogleOAuthCallback from '../google';

const mockAuthService = AuthService as jest.Mocked<typeof AuthService>;

const wrapper = ({ children }: { children: React.ReactNode }) => (
  <PaperProvider>{children}</PaperProvider>
);

beforeEach(async () => {
  jest.clearAllMocks();
  mockDispatch.mockClear();
  mockCode = 'auth-code-from-google-123';
  await AsyncStorage.setItem('oauth_code_verifier', 'test-code-verifier-xyz');
  await AsyncStorage.setItem(
    'oauth_redirect_uri',
    'com.googleusercontent.apps.545148539649-m12d16iqkq3vvjorm7aqjftkohlmuibo:/oauth2redirect/google',
  );
  mockFetch.mockResolvedValue({
    ok: true,
    json: async () => ({ id_token: 'eyJhbGciOiJSUzI1NiJ9.exchanged-id-token' }),
  });
});

describe('GoogleOAuthCallback', () => {
  it('exibe indicador de carregamento ao montar', () => {
    mockAuthService.verifyGoogleIdToken.mockImplementation(() => new Promise(jest.fn()));

    render(<GoogleOAuthCallback />, { wrapper });

    expect(screen.getByTestId('google-callback-loading')).toBeTruthy();
  });

  it('troca o code pelo id_token e chama verifyGoogleIdToken', async () => {
    mockAuthService.verifyGoogleIdToken.mockResolvedValueOnce(undefined);

    render(<GoogleOAuthCallback />, { wrapper });

    await waitFor(() => {
      expect(mockFetch).toHaveBeenCalledWith(
        'https://oauth2.googleapis.com/token',
        expect.objectContaining({ method: 'POST' }),
      );
      expect(mockAuthService.verifyGoogleIdToken).toHaveBeenCalledWith(
        'eyJhbGciOiJSUzI1NiJ9.exchanged-id-token',
      );
    });
  });

  it('navega para /(app) após login bem-sucedido', async () => {
    mockAuthService.verifyGoogleIdToken.mockResolvedValueOnce(undefined);

    render(<GoogleOAuthCallback />, { wrapper });

    await waitFor(() => {
      expect(mockDispatch).toHaveBeenCalled();
    });
  });

  it('redireciona para login quando não há code na url', async () => {
    mockCode = undefined;

    render(<GoogleOAuthCallback />, { wrapper });

    await waitFor(() => {
      expect(mockReplace).toHaveBeenCalledWith('/(auth)/login');
    });
    expect(mockFetch).not.toHaveBeenCalled();
    expect(mockAuthService.verifyGoogleIdToken).not.toHaveBeenCalled();
  });

  it('redireciona para login com erro quando troca de código falha', async () => {
    mockFetch.mockResolvedValueOnce({ ok: false, json: async () => ({}) });

    render(<GoogleOAuthCallback />, { wrapper });

    await waitFor(() => {
      expect(mockReplace).toHaveBeenCalledWith({
        pathname: '/(auth)/login',
        params: { error: 'Falha na troca do código OAuth' },
      });
    });
  });

  it('redireciona para login com erro quando id_token está ausente na resposta', async () => {
    mockFetch.mockResolvedValueOnce({ ok: true, json: async () => ({}) });

    render(<GoogleOAuthCallback />, { wrapper });

    await waitFor(() => {
      expect(mockReplace).toHaveBeenCalledWith({
        pathname: '/(auth)/login',
        params: { error: 'id_token ausente na resposta' },
      });
    });
  });

  it('redireciona para login com erro quando verifyGoogleIdToken falha', async () => {
    mockAuthService.verifyGoogleIdToken.mockRejectedValueOnce(new Error('Token inválido'));

    render(<GoogleOAuthCallback />, { wrapper });

    await waitFor(() => {
      expect(mockReplace).toHaveBeenCalledWith({
        pathname: '/(auth)/login',
        params: { error: 'Token inválido' },
      });
    });
  });

  it('redireciona para login com noTenantError quando usuário não tem tenant', async () => {
    mockAuthService.verifyGoogleIdToken.mockRejectedValueOnce(
      new Error(
        'Your account is not associated with any tenant. Contact your administrator.',
      ),
    );

    render(<GoogleOAuthCallback />, { wrapper });

    await waitFor(() => {
      expect(mockReplace).toHaveBeenCalledWith({
        pathname: '/(auth)/login',
        params: { noTenantError: 'true' },
      });
    });
  });
});
