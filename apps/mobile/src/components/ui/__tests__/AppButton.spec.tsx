import React from 'react';
import { fireEvent, render, screen } from '@testing-library/react-native';
import { PaperProvider } from 'react-native-paper';
import { AppButton } from '../AppButton';

const wrapper = ({ children }: { children: React.ReactNode }) => (
  <PaperProvider>{children}</PaperProvider>
);

describe('AppButton', () => {
  it('renderiza com o label correto', () => {
    render(<AppButton label="Entrar" onPress={jest.fn()} />, { wrapper });
    expect(screen.getByText('Entrar')).toBeTruthy();
  });

  it('chama onPress ao tocar', () => {
    const onPress = jest.fn();
    render(<AppButton label="Entrar" onPress={onPress} />, { wrapper });
    fireEvent.press(screen.getByText('Entrar'));
    expect(onPress).toHaveBeenCalledTimes(1);
  });

  it('não chama onPress quando disabled é true', () => {
    const onPress = jest.fn();
    render(<AppButton label="Entrar" onPress={onPress} disabled />, { wrapper });
    fireEvent.press(screen.getByText('Entrar'));
    expect(onPress).not.toHaveBeenCalled();
  });

  it('renderiza indicador de loading quando loading é true', () => {
    render(<AppButton label="Entrar" onPress={jest.fn()} loading />, { wrapper });
    expect(screen.getByTestId('app-button-loading')).toBeTruthy();
  });

  it('aceita variant outline', () => {
    render(<AppButton label="Entrar" onPress={jest.fn()} variant="outline" />, { wrapper });
    expect(screen.getByText('Entrar')).toBeTruthy();
  });
});
