import React from 'react';
import { act, fireEvent, render, screen, waitFor } from '@testing-library/react-native';
import { PaperProvider } from 'react-native-paper';
import { LoginScreen } from '../LoginScreen';
import { AuthService } from '../../../services/auth.service';

jest.mock('../../../services/auth.service');

const mockPush = jest.fn();
const mockReplace = jest.fn();
jest.mock('expo-router', () => ({
  useRouter: () => ({ push: mockPush, replace: mockReplace }),
}));

jest.mock('expo-auth-session', () => ({
  makeRedirectUri: () =>
    'com.googleusercontent.apps.545148539649-m12d16iqkq3vvjorm7aqjftkohlmuibo:/oauth2redirect/google',
}));

const mockSetSession = jest.fn().mockResolvedValue(undefined);
jest.mock('../../../hooks/useAuth', () => ({
  useAuth: () => ({ setSession: mockSetSession }),
}));

let mockGoogleResponse: { type: string; params?: { code: string } } | null = null;
const mockPromptAsync = jest.fn();
jest.mock('expo-auth-session/providers/google', () => ({
  useAuthRequest: () => [null, mockGoogleResponse, mockPromptAsync],
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
    it('chama loginWithGoogle com code e redirectUri proxy ao receber resposta success', async () => {
      mockGoogleResponse = { type: 'success', params: { code: 'auth-code-123' } };
      mockAuthService.loginWithGoogle.mockResolvedValue(undefined);

      render(<LoginScreen />, { wrapper });

      await waitFor(() => {
        expect(mockAuthService.loginWithGoogle).toHaveBeenCalledWith(
          'auth-code-123',
          'com.googleusercontent.apps.545148539649-m12d16iqkq3vvjorm7aqjftkohlmuibo:/oauth2redirect/google',
        );
      });
    });

    it('chama setSession e navega para /(app) após login google bem-sucedido', async () => {
      mockGoogleResponse = { type: 'success', params: { code: 'auth-code-123' } };
      mockAuthService.loginWithGoogle.mockResolvedValue(undefined);

      render(<LoginScreen />, { wrapper });

      await waitFor(() => {
        expect(mockSetSession).toHaveBeenCalledWith(true);
        expect(mockReplace).toHaveBeenCalledWith('/(app)');
      });
    });

    it('exibe erro de api quando loginWithGoogle falha', async () => {
      mockGoogleResponse = { type: 'success', params: { code: 'auth-code-123' } };
      mockAuthService.loginWithGoogle.mockRejectedValueOnce(new Error('Conta não encontrada'));

      render(<LoginScreen />, { wrapper });

      await waitFor(() => {
        expect(screen.getByText('Conta não encontrada')).toBeTruthy();
      });
    });

    it('não chama loginWithGoogle quando resposta google não é success', async () => {
      mockGoogleResponse = { type: 'cancel' };

      render(<LoginScreen />, { wrapper });

      await waitFor(() => {
        expect(mockAuthService.loginWithGoogle).not.toHaveBeenCalled();
      });
    });
  });
});
