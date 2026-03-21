import React from 'react';
import { render, fireEvent } from '@testing-library/react-native';
import { Button } from './Button';

describe('Button', () => {
  it('renders the label', () => {
    const { getByText } = render(<Button label="Entrar" onPress={() => {}} />);
    expect(getByText('Entrar')).toBeTruthy();
  });

  it('calls onPress when tapped', () => {
    const onPress = jest.fn();
    const { getByText } = render(<Button label="Enviar" onPress={onPress} />);
    fireEvent.press(getByText('Enviar'));
    expect(onPress).toHaveBeenCalledTimes(1);
  });

  it('does not call onPress when disabled', () => {
    const onPress = jest.fn();
    const { getByText } = render(
      <Button label="Enviar" onPress={onPress} disabled />
    );
    fireEvent.press(getByText('Enviar'));
    expect(onPress).not.toHaveBeenCalled();
  });

  it('shows loading indicator and hides label when loading', () => {
    const { getByTestId, queryByText } = render(
      <Button label="Enviar" onPress={() => {}} loading />
    );
    expect(getByTestId('button-loading')).toBeTruthy();
    expect(queryByText('Enviar')).toBeNull();
  });

  it('does not call onPress when loading', () => {
    const onPress = jest.fn();
    const { getByTestId } = render(
      <Button label="Enviar" onPress={onPress} loading />
    );
    fireEvent.press(getByTestId('button-loading'));
    expect(onPress).not.toHaveBeenCalled();
  });

  it('renders ghost variant with correct testID', () => {
    const { getByTestId } = render(
      <Button label="Google" onPress={() => {}} variant="ghost" testID="btn-google" />
    );
    expect(getByTestId('btn-google')).toBeTruthy();
  });
});
