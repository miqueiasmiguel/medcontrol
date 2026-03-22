import React from 'react';
import { render, screen, waitFor } from '@testing-library/react-native';
import { PaperProvider } from 'react-native-paper';

jest.mock('../../../src/services/auth.service');
jest.mock('@react-native-async-storage/async-storage', () =>
  require('@react-native-async-storage/async-storage/jest/async-storage-mock'),
);

const mockReplace = jest.fn();
let mockCode: string | undefined = 'google-auth-code-123';

jest.mock('expo-router', () => ({
  useRouter: () => ({ replace: mockReplace }),
  useLocalSearchParams: () => ({ code: mockCode }),
}));

jest.mock('expo-auth-session', () => ({
  makeRedirectUri: () =>
    'com.googleusercontent.apps.545148539649-m12d16iqkq3vvjorm7aqjftkohlmuibo:/oauth2redirect/google',
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

import { AuthService } from '../../../src/services/auth.service';
import GoogleOAuthCallback from '../google';

const mockAuthService = AuthService as jest.Mocked<typeof AuthService>;

const wrapper = ({ children }: { children: React.ReactNode }) => (
  <PaperProvider>{children}</PaperProvider>
);

beforeEach(() => {
  jest.clearAllMocks();
  mockCode = 'google-auth-code-123';
});

describe('GoogleOAuthCallback', () => {
  it('exibe indicador de carregamento ao montar', () => {
    mockAuthService.loginWithGoogle.mockImplementation(() => new Promise(() => {}));

    render(<GoogleOAuthCallback />, { wrapper });

    expect(screen.getByTestId('google-callback-loading')).toBeTruthy();
  });

  it('chama loginWithGoogle com code e redirectUri corretos', async () => {
    mockAuthService.loginWithGoogle.mockResolvedValueOnce({
      accessToken: 'token',
      refreshToken: 'refresh',
      expiresIn: 3600,
      tokenType: 'Bearer',
    });

    render(<GoogleOAuthCallback />, { wrapper });

    await waitFor(() => {
      expect(mockAuthService.loginWithGoogle).toHaveBeenCalledWith(
        'google-auth-code-123',
        'com.googleusercontent.apps.545148539649-m12d16iqkq3vvjorm7aqjftkohlmuibo:/oauth2redirect/google',
      );
    });
  });

  it('navega para /(app) após login bem-sucedido', async () => {
    mockAuthService.loginWithGoogle.mockResolvedValueOnce({
      accessToken: 'token',
      refreshToken: 'refresh',
      expiresIn: 3600,
      tokenType: 'Bearer',
    });

    render(<GoogleOAuthCallback />, { wrapper });

    await waitFor(() => {
      expect(mockReplace).toHaveBeenCalledWith('/(app)');
    });
  });

  it('redireciona para login quando não há code na url', async () => {
    mockCode = undefined;

    render(<GoogleOAuthCallback />, { wrapper });

    await waitFor(() => {
      expect(mockReplace).toHaveBeenCalledWith('/(auth)/login');
    });
    expect(mockAuthService.loginWithGoogle).not.toHaveBeenCalled();
  });

  it('redireciona para login quando loginWithGoogle falha', async () => {
    mockAuthService.loginWithGoogle.mockRejectedValueOnce(new Error('Token inválido'));

    render(<GoogleOAuthCallback />, { wrapper });

    await waitFor(() => {
      expect(mockReplace).toHaveBeenCalledWith('/(auth)/login');
    });
  });
});
