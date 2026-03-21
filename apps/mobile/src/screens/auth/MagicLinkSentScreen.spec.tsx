import React from 'react';
import { render, fireEvent } from '@testing-library/react-native';
import { MagicLinkSentScreen } from './MagicLinkSentScreen';

const mockNavigate = jest.fn();
const mockGoBack = jest.fn();
const mockNavigation = {
  navigate: mockNavigate,
  goBack: mockGoBack,
  dispatch: jest.fn(),
  reset: jest.fn(),
  canGoBack: jest.fn(() => true),
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

const mockRoute = {
  key: 'MagicLinkSent',
  name: 'MagicLinkSent',
  params: { email: 'dr@clinica.com' },
};

describe('MagicLinkSentScreen', () => {
  beforeEach(() => jest.clearAllMocks());

  it('renders the confirmation title', () => {
    const { getByText } = render(
      <MagicLinkSentScreen
        navigation={mockNavigation as any}
        route={mockRoute as any}
      />
    );
    expect(getByText('Verifique seu e-mail')).toBeTruthy();
  });

  it('renders the email from route params', () => {
    const { getByText } = render(
      <MagicLinkSentScreen
        navigation={mockNavigation as any}
        route={mockRoute as any}
      />
    );
    expect(getByText('dr@clinica.com')).toBeTruthy();
  });

  it('renders the TTL hint text', () => {
    const { getByText } = render(
      <MagicLinkSentScreen
        navigation={mockNavigation as any}
        route={mockRoute as any}
      />
    );
    expect(getByText(/15 minutos/)).toBeTruthy();
  });

  it('navigates back when retry button is pressed', () => {
    const { getByTestId } = render(
      <MagicLinkSentScreen
        navigation={mockNavigation as any}
        route={mockRoute as any}
      />
    );
    fireEvent.press(getByTestId('btn-retry'));
    expect(mockGoBack).toHaveBeenCalledTimes(1);
  });
});
