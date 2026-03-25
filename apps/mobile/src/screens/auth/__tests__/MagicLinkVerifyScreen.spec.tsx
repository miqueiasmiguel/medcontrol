import React from 'react';
import { render, screen, waitFor } from '@testing-library/react-native';
import { PaperProvider } from 'react-native-paper';
import { MagicLinkVerifyScreen } from '../MagicLinkVerifyScreen';
import { AuthService } from '../../../services/auth.service';

jest.mock('../../../services/auth.service');
jest.mock('@react-native-async-storage/async-storage', () =>
  require('@react-native-async-storage/async-storage/jest/async-storage-mock'),
);

const mockReplace = jest.fn();
const mockDispatch = jest.fn();
let mockToken: string | undefined = 'valid-token-123';

jest.mock('expo-router', () => ({
  useRouter: () => ({ replace: mockReplace }),
  useLocalSearchParams: () => ({ token: mockToken }),
  useRootNavigation: () => ({ dispatch: mockDispatch }),
}));

jest.mock('@react-navigation/native', () => ({
  CommonActions: {
    reset: jest.fn((state: unknown) => ({ type: 'RESET', state })),
  },
}));

jest.mock('../../../hooks/useAuth', () => ({
  useAuth: () => ({ setSession: jest.fn().mockResolvedValue(undefined) }),
}));

const mockAuthService = AuthService as jest.Mocked<typeof AuthService>;

const wrapper = ({ children }: { children: React.ReactNode }) => (
  <PaperProvider>{children}</PaperProvider>
);

beforeEach(() => {
  jest.clearAllMocks();
  mockDispatch.mockClear();
  mockToken = 'valid-token-123';
});

describe('MagicLinkVerifyScreen', () => {
  it('exibe indicador de carregamento ao montar', () => {
    mockAuthService.verifyMagicLink.mockImplementation(() => new Promise(jest.fn()));

    render(<MagicLinkVerifyScreen />, { wrapper });

    expect(screen.getByTestId('verify-loading')).toBeTruthy();
  });

  it('chama AuthService.verifyMagicLink com o token da url', async () => {
    mockAuthService.verifyMagicLink.mockResolvedValueOnce(undefined);

    render(<MagicLinkVerifyScreen />, { wrapper });

    await waitFor(() => {
      expect(mockAuthService.verifyMagicLink).toHaveBeenCalledWith('valid-token-123');
    });
  });

  it('navega para o app após verificação bem-sucedida', async () => {
    mockAuthService.verifyMagicLink.mockResolvedValueOnce(undefined);

    render(<MagicLinkVerifyScreen />, { wrapper });

    await waitFor(() => {
      expect(mockDispatch).toHaveBeenCalled();
    });
  });

  it('exibe mensagem de erro quando verificação falha', async () => {
    mockAuthService.verifyMagicLink.mockRejectedValueOnce(new Error('Token inválido ou expirado'));

    render(<MagicLinkVerifyScreen />, { wrapper });

    await waitFor(() => {
      expect(screen.getByText('Token inválido ou expirado')).toBeTruthy();
    });
  });

  it('exibe erro quando token não está presente na url', async () => {
    mockToken = undefined;

    render(<MagicLinkVerifyScreen />, { wrapper });

    await waitFor(() => {
      expect(screen.getByText('Link inválido. Solicite um novo.')).toBeTruthy();
    });
    expect(mockAuthService.verifyMagicLink).not.toHaveBeenCalled();
  });
});
