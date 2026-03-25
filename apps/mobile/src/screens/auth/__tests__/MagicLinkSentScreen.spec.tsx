import React from 'react';
import { fireEvent, render, screen } from '@testing-library/react-native';
import { PaperProvider } from 'react-native-paper';
import { MagicLinkSentScreen } from '../MagicLinkSentScreen';

jest.mock('@react-native-async-storage/async-storage', () =>
  require('@react-native-async-storage/async-storage/jest/async-storage-mock'),
);

const mockBack = jest.fn();
jest.mock('expo-router', () => ({
  useRouter: () => ({ back: mockBack }),
  useLocalSearchParams: () => ({ email: 'user@test.com' }),
}));

const wrapper = ({ children }: { children: React.ReactNode }) => (
  <PaperProvider>{children}</PaperProvider>
);

beforeEach(() => jest.clearAllMocks());

describe('MagicLinkSentScreen', () => {
  it('exibe o email do destinatário na mensagem de confirmação', () => {
    render(<MagicLinkSentScreen />, { wrapper });
    expect(screen.getByText('user@test.com')).toBeTruthy();
  });

  it('informa que o link é válido por 15 minutos', () => {
    render(<MagicLinkSentScreen />, { wrapper });
    expect(screen.getByText(/15 minutos/i)).toBeTruthy();
  });

  it('tem botão para voltar ao login', () => {
    render(<MagicLinkSentScreen />, { wrapper });
    expect(screen.getByText('Voltar ao Login')).toBeTruthy();
  });

  it('volta para login ao tocar no botão', () => {
    render(<MagicLinkSentScreen />, { wrapper });
    fireEvent.press(screen.getByText('Voltar ao Login'));
    expect(mockBack).toHaveBeenCalledTimes(1);
  });
});
