import React from 'react';
import { render, fireEvent, waitFor } from '@testing-library/react-native';
import { LoginScreen } from './LoginScreen';

const mockNavigate = jest.fn();
const mockNavigation = {
  navigate: mockNavigate,
  goBack: jest.fn(),
  dispatch: jest.fn(),
  reset: jest.fn(),
  canGoBack: jest.fn(),
  isFocused: jest.fn(),
  getId: jest.fn(),
  getParent: jest.fn(),
  getState: jest.fn(),
  setParams: jest.fn(),
  setOptions: jest.fn(),
  addListener: jest.fn(() => jest.fn()),
  removeListener: jest.fn(),
  replace: jest.fn(),
  push: jest.fn(),
  pop: jest.fn(),
  popToTop: jest.fn(),
};

const mockRoute = { key: 'Login', name: 'Login', params: undefined };

describe('LoginScreen', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('renders the MedControl logo text', () => {
    const { getByText } = render(
      <LoginScreen navigation={mockNavigation as any} route={mockRoute as any} />
    );
    expect(getByText('MedControl')).toBeTruthy();
  });

  it('renders the email input', () => {
    const { getByPlaceholderText } = render(
      <LoginScreen navigation={mockNavigation as any} route={mockRoute as any} />
    );
    expect(getByPlaceholderText('seu@email.com')).toBeTruthy();
  });

  it('renders the magic link submit button', () => {
    const { getByTestId } = render(
      <LoginScreen navigation={mockNavigation as any} route={mockRoute as any} />
    );
    expect(getByTestId('btn-magic-link')).toBeTruthy();
  });

  it('renders the Google sign-in button', () => {
    const { getByTestId } = render(
      <LoginScreen navigation={mockNavigation as any} route={mockRoute as any} />
    );
    expect(getByTestId('btn-google')).toBeTruthy();
  });

  it('updates email state when typing', () => {
    const { getByPlaceholderText } = render(
      <LoginScreen navigation={mockNavigation as any} route={mockRoute as any} />
    );
    const input = getByPlaceholderText('seu@email.com');
    fireEvent.changeText(input, 'medico@clinica.com.br');
    expect(input.props.value).toBe('medico@clinica.com.br');
  });

  it('shows validation error when submitting with invalid email', async () => {
    const { getByTestId, getByText } = render(
      <LoginScreen navigation={mockNavigation as any} route={mockRoute as any} />
    );
    fireEvent.press(getByTestId('btn-magic-link'));
    await waitFor(() => {
      expect(getByText('Informe um e-mail válido')).toBeTruthy();
    });
  });

  it('navigates to MagicLinkSent on valid email submit', async () => {
    const { getByPlaceholderText, getByTestId } = render(
      <LoginScreen navigation={mockNavigation as any} route={mockRoute as any} />
    );
    fireEvent.changeText(getByPlaceholderText('seu@email.com'), 'dr@clinica.com');
    fireEvent.press(getByTestId('btn-magic-link'));
    await waitFor(() => {
      expect(mockNavigate).toHaveBeenCalledWith('MagicLinkSent', {
        email: 'dr@clinica.com',
      });
    });
  });
});
