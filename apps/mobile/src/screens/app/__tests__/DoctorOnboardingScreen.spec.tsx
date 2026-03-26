import React from 'react';
import { render, fireEvent, waitFor } from '@testing-library/react-native';
import DoctorOnboardingScreen from '../DoctorOnboardingScreen';
import { UserService } from '../../../services/user.service';

jest.mock('@react-native-async-storage/async-storage', () =>
  require('@react-native-async-storage/async-storage/jest/async-storage-mock'),
);

jest.mock('../../../services/user.service');

const mockReplace = jest.fn();
jest.mock('expo-router', () => ({
  useRouter: () => ({ replace: mockReplace }),
}));

jest.mock('react-native-safe-area-context', () => ({
  useSafeAreaInsets: () => ({ top: 0, bottom: 0, left: 0, right: 0 }),
}));

const mockTheme = {
  colors: {
    primary: '#0EA5E9',
    primaryText: '#FFFFFF',
    secondary: '#0F172A',
    border: '#E2E8F0',
    borderStrong: '#94A3B8',
    text: { primary: '#0F172A', secondary: '#64748B', tertiary: '#94A3B8', onDark: '#FFFFFF', onDarkSubtle: 'rgba(255,255,255,0.7)', disabled: '#CBD5E1' },
    surface: { background: '#F8FAFC', card: '#FFFFFF' },
    error: { base: '#EF4444' },
  },
  typography: {
    fontSize: { sm: 14, md: 16, lg: 18 },
    fontWeight: { regular: '400', medium: '500', semibold: '600', bold: '700' },
  },
  spacing: new Proxy({}, { get: (_t: object, p: string | symbol) => Number(p) * 4 }),
  borderRadius: new Proxy({}, { get: () => 8 }),
};

jest.mock('@medcontrol/design-system/native', () => ({
  useTheme: () => mockTheme,
  theme: mockTheme,
  darkTheme: mockTheme,
}));

jest.mock('../../../contexts/ThemeContext', () => ({
  useAppTheme: () => mockTheme,
}));

jest.mock('react-native-paper', () => {
  const { View, Text, TextInput, TouchableOpacity } = jest.requireActual('react-native');
  return {
    TextInput: ({ testID, value, onChangeText, label, ...props }: { testID?: string; value: string; onChangeText: (v: string) => void; label: string; [key: string]: unknown }) => (
      <TextInput testID={testID} value={value} onChangeText={onChangeText} placeholder={label} {...props} />
    ),
    HelperText: ({ children, visible }: { children: React.ReactNode; visible: boolean }) =>
      visible ? <Text>{children}</Text> : null,
    ActivityIndicator: () => <View testID="spinner" />,
    Button: ({ testID, onPress, children, disabled }: { testID?: string; onPress: () => void; children: React.ReactNode; disabled?: boolean }) => (
      <TouchableOpacity testID={testID} onPress={onPress} disabled={disabled}>
        <Text>{children}</Text>
      </TouchableOpacity>
    ),
    Text: ({ children, ...props }: { children: React.ReactNode; [key: string]: unknown }) => {
      const { Text: RNText } = jest.requireActual('react-native');
      return <RNText {...props}>{children}</RNText>;
    },
  };
});

jest.mock('@expo/vector-icons', () => ({ Ionicons: () => null }));

const mockProfile = {
  id: 'doc-1',
  tenantId: 'tenant-1',
  userId: 'user-1',
  name: 'Dr. João Silva',
  crm: '123456',
  councilState: 'SP',
  specialty: 'Cardiologia',
};

beforeEach(() => {
  jest.clearAllMocks();
  mockReplace.mockReset();
});

describe('DoctorOnboardingScreen', () => {
  it('renders all form fields', () => {
    const { getByTestId } = render(<DoctorOnboardingScreen />);
    expect(getByTestId('field-name')).toBeTruthy();
    expect(getByTestId('field-crm')).toBeTruthy();
    expect(getByTestId('field-councilState')).toBeTruthy();
    expect(getByTestId('field-specialty')).toBeTruthy();
  });

  it('shows validation errors when submitting empty form', async () => {
    const { getByTestId, findByText } = render(<DoctorOnboardingScreen />);
    fireEvent.press(getByTestId('submit-button'));
    expect(await findByText('Nome profissional é obrigatório')).toBeTruthy();
    expect(UserService.createMyDoctorProfile).not.toHaveBeenCalled();
  });

  it('calls createMyDoctorProfile and navigates to /(app) on success', async () => {
    (UserService.createMyDoctorProfile as jest.Mock).mockResolvedValue(mockProfile);

    const { getByTestId } = render(<DoctorOnboardingScreen />);

    fireEvent.changeText(getByTestId('field-name'), 'Dr. João Silva');
    fireEvent.changeText(getByTestId('field-crm'), '123456');
    fireEvent.changeText(getByTestId('field-councilState'), 'SP');
    fireEvent.changeText(getByTestId('field-specialty'), 'Cardiologia');
    fireEvent.press(getByTestId('submit-button'));

    await waitFor(() => {
      expect(UserService.createMyDoctorProfile).toHaveBeenCalledWith({
        name: 'Dr. João Silva',
        crm: '123456',
        councilState: 'SP',
        specialty: 'Cardiologia',
      });
      expect(mockReplace).toHaveBeenCalledWith('/(app)');
    });
  });

  it('shows error alert on API failure', async () => {
    (UserService.createMyDoctorProfile as jest.Mock).mockRejectedValue(new Error('Erro de rede'));
    const alertSpy = jest.spyOn(require('react-native').Alert, 'alert');

    const { getByTestId } = render(<DoctorOnboardingScreen />);

    fireEvent.changeText(getByTestId('field-name'), 'Dr. João Silva');
    fireEvent.changeText(getByTestId('field-crm'), '123456');
    fireEvent.changeText(getByTestId('field-councilState'), 'SP');
    fireEvent.changeText(getByTestId('field-specialty'), 'Cardiologia');
    fireEvent.press(getByTestId('submit-button'));

    await waitFor(() => {
      expect(alertSpy).toHaveBeenCalledWith('Erro', expect.stringContaining('Erro de rede'));
    });
  });

  it('sets AsyncStorage skip flag and navigates to /(app) when skipping', async () => {
    const AsyncStorage = require('@react-native-async-storage/async-storage');
    const { getByTestId } = render(<DoctorOnboardingScreen />);

    fireEvent.press(getByTestId('skip-button'));

    await waitFor(() => {
      expect(AsyncStorage.setItem).toHaveBeenCalledWith('mmc_onboarding_skip', '1');
      expect(mockReplace).toHaveBeenCalledWith('/(app)');
    });
  });
});
