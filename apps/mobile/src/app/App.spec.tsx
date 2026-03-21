import React from 'react';
import { render } from '@testing-library/react-native';
import App from './App';

// Mock RootNavigator to render LoginScreen directly without native navigation deps
jest.mock('../navigation/RootNavigator', () => ({
  RootNavigator: () => {
    const { LoginScreen } = require('../screens/auth/LoginScreen');
    const mockNav = {
      navigate: jest.fn(),
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
    // eslint-disable-next-line @typescript-eslint/no-require-imports
    const React = require('react');
    return React.createElement(LoginScreen, { navigation: mockNav, route: mockRoute });
  },
}));

test('renders the login screen', () => {
  const { getByText } = render(<App />);
  expect(getByText('MedControl')).toBeTruthy();
});
