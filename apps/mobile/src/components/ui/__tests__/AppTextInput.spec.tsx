import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react-native';
import { PaperProvider } from 'react-native-paper';
import { AppTextInput } from '../AppTextInput';

jest.mock('@react-native-async-storage/async-storage', () =>
  require('@react-native-async-storage/async-storage/jest/async-storage-mock'),
);

jest.mock('../../../contexts/ThemeContext', () => ({
  useAppTheme: () => ({
    colors: {
      primary: '#F97316',
      border: '#E9ECEF',
      text: { primary: '#212529', secondary: '#868E96', tertiary: '#ADB5BD' },
      surface: { card: '#FFFFFF' },
    },
  }),
}));

const wrapper = ({ children }: { children: React.ReactNode }) => (
  <PaperProvider>{children}</PaperProvider>
);

describe('AppTextInput', () => {
  it('renderiza com o placeholder correto', () => {
    render(<AppTextInput label="Email" value="" onChangeText={jest.fn()} placeholder="seu@email.com" />, { wrapper });
    expect(screen.getByPlaceholderText('seu@email.com')).toBeTruthy();
  });

  it('exibe mensagem de erro quando errorMessage é passado', () => {
    render(
      <AppTextInput label="Email" value="" onChangeText={jest.fn()} errorMessage="Email inválido" />,
      { wrapper },
    );
    expect(screen.getByText('Email inválido')).toBeTruthy();
  });

  it('chama onChangeText ao digitar', () => {
    const onChangeText = jest.fn();
    render(<AppTextInput label="Email" value="" onChangeText={onChangeText} placeholder="seu@email.com" />, { wrapper });
    fireEvent.changeText(screen.getByPlaceholderText('seu@email.com'), 'test@test.com');
    expect(onChangeText).toHaveBeenCalledWith('test@test.com');
  });

  it('não exibe mensagem de erro quando errorMessage não é passado', () => {
    render(<AppTextInput label="Email" value="" onChangeText={jest.fn()} placeholder="seu@email.com" />, { wrapper });
    expect(screen.queryByText('Email inválido')).toBeNull();
  });
});
