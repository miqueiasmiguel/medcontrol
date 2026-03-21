import React from 'react';
import { render, fireEvent } from '@testing-library/react-native';
import { TextInput } from './TextInput';

describe('TextInput', () => {
  it('renders the label', () => {
    const { getByText } = render(
      <TextInput label="E-mail" value="" onChangeText={jest.fn()} />
    );
    expect(getByText('E-mail')).toBeTruthy();
  });

  it('renders the placeholder', () => {
    const { getByPlaceholderText } = render(
      <TextInput
        label="E-mail"
        value=""
        onChangeText={jest.fn()}
        placeholder="seu@email.com"
      />
    );
    expect(getByPlaceholderText('seu@email.com')).toBeTruthy();
  });

  it('calls onChangeText when typing', () => {
    const onChangeText = jest.fn();
    const { getByPlaceholderText } = render(
      <TextInput
        label="E-mail"
        value=""
        onChangeText={onChangeText}
        placeholder="seu@email.com"
      />
    );
    fireEvent.changeText(getByPlaceholderText('seu@email.com'), 'test@test.com');
    expect(onChangeText).toHaveBeenCalledWith('test@test.com');
  });

  it('shows error message when error prop is provided', () => {
    const { getByText } = render(
      <TextInput
        label="E-mail"
        value=""
        onChangeText={jest.fn()}
        error="E-mail inválido"
      />
    );
    expect(getByText('E-mail inválido')).toBeTruthy();
  });

  it('does not show error message when error is absent', () => {
    const { queryByTestId } = render(
      <TextInput label="E-mail" value="" onChangeText={jest.fn()} />
    );
    expect(queryByTestId('input-error')).toBeNull();
  });
});
