import React from 'react';
import { act, fireEvent, render, screen, waitFor } from '@testing-library/react-native';
import { PaperProvider } from 'react-native-paper';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { LoginScreen } from '../LoginScreen';
import { AuthService } from '../../../services/auth.service';

jest.mock('../../../services/auth.service');

const mockPush = jest.fn();
const mockReplace = jest.fn();
let mockSearchParams: Record<string, string> = {};
jest.mock('expo-router', () => ({
  useRouter: () => ({ push: mockPush, replace: mockReplace }),
  useLocalSearchParams: () => mockSearchParams,
}));

jest.mock('expo-auth-session', () => ({
  makeRedirectUri: () => 'com.googleusercontent.apps.test:/oauth2redirect/google',
  ResponseType: { IdToken: 'id_token', Code: 'code' },
}));

jest.mock('@react-native-async-storage/async-storage', () =>
  require('@react-native-async-storage/async-storage/jest/async-storage-mock'),
);

let mockGoogleResponse: { type: string; params?: { code: string } } | null = null;
const mockPromptAsync = jest.fn();
const mockRequest = { codeVerifier: 'test-code-verifier-abc123' };
jest.mock('expo-auth-session/providers/google', () => ({
  useAuthRequest: () => [mockRequest, mockGoogleResponse, mockPromptAsync],
}));

jest.mock('expo-linking', () => ({
  createURL: jest.fn(() => 'medcontrol://'),
  resolveScheme: jest.fn(() => 'medcontrol'),
}));

jest.mock('@expo/vector-icons', () => ({
  AntDesign: () => null,
}));

const mockAuthService = AuthService as jest.Mocked<typeof AuthService>;

const wrapper = ({ children }: { children: React.ReactNode }) => (
  <PaperProvider>{children}</PaperProvider>
);

beforeEach(() => {
  jest.clearAllMocks();
  mockGoogleResponse = null;
  mockSearchParams = {};
});

describe('LoginScreen', () => {
  it('renderiza campo de email', () => {
    render(<LoginScreen />, { wrapper });
    expect(screen.getByPlaceholderText('seu@email.com')).toBeTruthy();
  });

  it('renderiza botão de continuar com email', () => {
    render(<LoginScreen />, { wrapper });
    expect(screen.getByText('Continuar com Email')).toBeTruthy();
  });

  it('renderiza botão de continuar com google', () => {
    render(<LoginScreen />, { wrapper });
    expect(screen.getByText('Continuar com Google')).toBeTruthy();
  });

  it('exibe erro de validação para email inválido ao submeter', async () => {
    render(<LoginScreen />, { wrapper });

    fireEvent.changeText(screen.getByPlaceholderText('seu@email.com'), 'nao-e-email');
    fireEvent.press(screen.getByText('Continuar com Email'));

    await waitFor(() => {
      expect(screen.getByText('Email inválido')).toBeTruthy();
    });
    expect(mockAuthService.sendMagicLink).not.toHaveBeenCalled();
  });

  it('exibe erro de validação quando email está vazio', async () => {
    render(<LoginScreen />, { wrapper });

    fireEvent.press(screen.getByText('Continuar com Email'));

    await waitFor(() => {
      expect(screen.getByText('Email é obrigatório')).toBeTruthy();
    });
  });

  it('chama AuthService.sendMagicLink com o email correto ao submeter', async () => {
    mockAuthService.sendMagicLink.mockResolvedValueOnce(undefined);
    render(<LoginScreen />, { wrapper });

    fireEvent.changeText(screen.getByPlaceholderText('seu@email.com'), 'user@test.com');
    fireEvent.press(screen.getByText('Continuar com Email'));

    await waitFor(() => {
      expect(mockAuthService.sendMagicLink).toHaveBeenCalledWith('user@test.com');
    });
  });

  it('navega para magic-link-sent após envio bem-sucedido', async () => {
    mockAuthService.sendMagicLink.mockResolvedValueOnce(undefined);
    render(<LoginScreen />, { wrapper });

    fireEvent.changeText(screen.getByPlaceholderText('seu@email.com'), 'user@test.com');
    fireEvent.press(screen.getByText('Continuar com Email'));

    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith(
        expect.objectContaining({ pathname: '/(auth)/magic-link-sent' }),
      );
    });
  });

  it('exibe mensagem de erro da api quando sendMagicLink falha', async () => {
    mockAuthService.sendMagicLink.mockRejectedValueOnce(new Error('Serviço indisponível'));
    render(<LoginScreen />, { wrapper });

    fireEvent.changeText(screen.getByPlaceholderText('seu@email.com'), 'user@test.com');
    fireEvent.press(screen.getByText('Continuar com Email'));

    await waitFor(() => {
      expect(screen.getByText('Serviço indisponível')).toBeTruthy();
    });
  });

  describe('Google OAuth', () => {
    it('chama promptAsync ao pressionar botão do google', async () => {
      render(<LoginScreen />, { wrapper });

      await act(async () => {
        fireEvent.press(screen.getByText('Continuar com Google'));
      });

      expect(mockPromptAsync).toHaveBeenCalled();
    });

    it('salva codeVerifier e redirectUri no AsyncStorage antes de chamar promptAsync', async () => {
      render(<LoginScreen />, { wrapper });

      await act(async () => {
        fireEvent.press(screen.getByText('Continuar com Google'));
      });

      expect(await AsyncStorage.getItem('oauth_code_verifier')).toBe('test-code-verifier-abc123');
      expect(await AsyncStorage.getItem('oauth_redirect_uri')).toBeTruthy();
    });

    it('não chama verifyGoogleIdToken diretamente — delega para google.tsx via roteamento do expo-router', async () => {
      // Garante que o handler de googleResponse foi removido do LoginScreen.
      // Antes: LoginScreen chamava loginWithGoogle diretamente, causando race condition
      // com google.tsx (segundo handler usava código já consumido e redirecionava para login).
      mockGoogleResponse = { type: 'success', params: { code: 'auth-code-123' } };

      render(<LoginScreen />, { wrapper });

      await act(async () => {});

      expect(mockAuthService.verifyGoogleIdToken).not.toHaveBeenCalled();
    });

    it('exibe erro de autenticação google vindo da url quando google.tsx redireciona com erro', async () => {
      mockSearchParams = { error: 'Google authentication failed.' };

      render(<LoginScreen />, { wrapper });

      expect(screen.getByText('Google authentication failed.')).toBeTruthy();
    });
  });
});
