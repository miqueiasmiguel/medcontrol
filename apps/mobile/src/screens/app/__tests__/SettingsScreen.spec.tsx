import React from 'react';
import { Alert } from 'react-native';
import { render, fireEvent, act, waitFor } from '@testing-library/react-native';
import SettingsScreen from '../SettingsScreen';
import { useAuth } from '../../../hooks/useAuth';
import { useCurrentUser } from '../../../hooks/useCurrentUser';
import { useDoctorProfile } from '../../../hooks/useDoctorProfile';
import { UserService } from '../../../services/user.service';
import { ThemePreferenceProvider } from '../../../contexts/ThemeContext';

// ── Mocks ────────────────────────────────────────────────────────────────────

jest.mock('@react-native-async-storage/async-storage', () =>
  require('@react-native-async-storage/async-storage/jest/async-storage-mock'),
);

jest.mock('../../../hooks/useAuth');
jest.mock('../../../hooks/useCurrentUser');
jest.mock('../../../hooks/useDoctorProfile');
jest.mock('../../../services/user.service');

jest.mock('@medcontrol/design-system/native', () => {
  const t = {
    colors: {
      primary: '#F97316',
      secondary: '#1B2E63',
      border: '#E9ECEF',
      error: { base: '#EF4444', light: '#FEF2F2' },
      text: {
        primary: '#212529',
        secondary: '#868E96',
        tertiary: '#ADB5BD',
        onDark: '#FFFFFF',
      },
      surface: { background: '#F8F9FA', card: '#FFFFFF' },
    },
    spacing: new Proxy({}, { get: (_obj: object, p: string | symbol) => Number(p) * 4 }),
    borderRadius: new Proxy({}, { get: () => 8 }),
    typography: {
      fontSize: { xs: 12, sm: 14, md: 16, lg: 18, xl: 20 },
      fontWeight: { regular: '400', medium: '500', semibold: '600', bold: '700' },
    },
    shadows: { sm: {} },
    components: { avatarMd: 40, inputHeight: 48 },
  };
  return { useTheme: () => t, theme: t, darkTheme: t };
});

jest.mock('react-native-safe-area-context', () => ({
  useSafeAreaInsets: () => ({ top: 0, bottom: 0, left: 0, right: 0 }),
}));

jest.mock('react-native-paper', () => {
  const { Text, TextInput } = jest.requireActual('react-native');
  return {
    Text: ({ children, ...props }: { children: React.ReactNode; [key: string]: unknown }) => (
      <Text {...props}>{children}</Text>
    ),
    TextInput: (props: object) => <TextInput {...props} />,
    HelperText: ({ children, ...props }: { children: React.ReactNode; [key: string]: unknown }) => (
      <Text {...props}>{children}</Text>
    ),
    ActivityIndicator: () => null,
    Button: ({
      children,
      onPress,
      ...props
    }: {
      children: React.ReactNode;
      onPress?: () => void;
      [key: string]: unknown;
    }) => (
      <Text onPress={onPress} {...props}>
        {children}
      </Text>
    ),
  };
});

jest.mock('@expo/vector-icons', () => ({
  Ionicons: () => null,
}));

jest.mock('expo-constants', () => ({
  default: { expoConfig: { extra: { apiUrl: 'http://localhost:5000' } } },
}));

const mockReplace = jest.fn();
const mockBack = jest.fn();
jest.mock('expo-router', () => ({
  useRouter: () => ({ replace: mockReplace, back: mockBack }),
}));

// ── Helpers ───────────────────────────────────────────────────────────────────

const mockLogout = jest.fn();
const mockUserService = UserService as jest.Mocked<typeof UserService>;

const fakeUser = {
  id: 'u1',
  email: 'joao@clinica.com',
  displayName: 'Dr. João',
  isEmailVerified: true,
  globalRole: 'user',
};

const fakeProfile = {
  id: 'd1',
  tenantId: 't1',
  userId: 'u1',
  name: 'Dr. João Silva',
  crm: '123456',
  councilState: 'SP',
  specialty: 'Cardiologia',
};

function setup() {
  jest.mocked(useAuth).mockReturnValue({
    isAuthenticated: true,
    isLoading: false,
    logout: mockLogout,
    setSession: jest.fn(),
  });
  jest.mocked(useCurrentUser).mockReturnValue({
    user: fakeUser,
    loading: false,
    error: null,
  });
  jest.mocked(useDoctorProfile).mockReturnValue({
    doctorProfile: fakeProfile,
    loading: false,
    error: null,
    refetch: jest.fn(),
  });
  mockUserService.updateProfile.mockResolvedValue(fakeUser);
  mockUserService.updateMyDoctorProfile.mockResolvedValue([fakeProfile]);
}

function renderScreen() {
  return render(
    <ThemePreferenceProvider>
      <SettingsScreen />
    </ThemePreferenceProvider>,
  );
}

beforeEach(() => {
  jest.clearAllMocks();
  setup();
  jest.spyOn(Alert, 'alert').mockImplementation(() => undefined);
  mockReplace.mockReset();
});

// ── Testes ────────────────────────────────────────────────────────────────────

describe('SettingsScreen — perfil', () => {
  it('renderiza todos os campos do perfil', () => {
    const { getByTestId } = renderScreen();

    expect(getByTestId('field-displayName')).toBeTruthy();
    expect(getByTestId('field-name')).toBeTruthy();
    expect(getByTestId('field-crm')).toBeTruthy();
    expect(getByTestId('field-councilState')).toBeTruthy();
    expect(getByTestId('field-specialty')).toBeTruthy();
  });

  it('chama updateProfile e updateMyDoctorProfile ao salvar', async () => {
    const { getByTestId } = renderScreen();

    await act(async () => {
      fireEvent.press(getByTestId('submit-button'));
    });

    await waitFor(() => {
      expect(mockUserService.updateProfile).toHaveBeenCalledTimes(1);
      expect(mockUserService.updateMyDoctorProfile).toHaveBeenCalledTimes(1);
    });
  });

  it('exibe alerta de sucesso após salvar', async () => {
    const { getByTestId } = renderScreen();

    await act(async () => {
      fireEvent.press(getByTestId('submit-button'));
    });

    await waitFor(() => {
      expect(Alert.alert).toHaveBeenCalledWith(
        expect.stringMatching(/sucesso/i),
        expect.any(String),
      );
    });
  });
});

describe('SettingsScreen — tema', () => {
  it('renderiza as três opções de tema', () => {
    const { getByTestId } = renderScreen();

    expect(getByTestId('theme-option-system')).toBeTruthy();
    expect(getByTestId('theme-option-light')).toBeTruthy();
    expect(getByTestId('theme-option-dark')).toBeTruthy();
  });

  it('persiste a preferência no AsyncStorage ao selecionar tema claro', async () => {
    const AsyncStorage = require('@react-native-async-storage/async-storage');
    const { getByTestId } = renderScreen();

    await act(async () => {
      fireEvent.press(getByTestId('theme-option-light'));
    });

    expect(AsyncStorage.setItem).toHaveBeenCalledWith('mmc_theme', 'light');
  });

  it('persiste a preferência no AsyncStorage ao selecionar tema escuro', async () => {
    const AsyncStorage = require('@react-native-async-storage/async-storage');
    const { getByTestId } = renderScreen();

    await act(async () => {
      fireEvent.press(getByTestId('theme-option-dark'));
    });

    expect(AsyncStorage.setItem).toHaveBeenCalledWith('mmc_theme', 'dark');
  });
});

describe('SettingsScreen — logout', () => {
  it('renderiza o botão de sair', () => {
    const { getByTestId } = renderScreen();
    expect(getByTestId('logout-button')).toBeTruthy();
  });

  it('exibe Alert de confirmação ao pressionar sair', () => {
    const { getByTestId } = renderScreen();
    fireEvent.press(getByTestId('logout-button'));

    expect(Alert.alert).toHaveBeenCalledWith(
      'Sair',
      expect.any(String),
      expect.arrayContaining([
        expect.objectContaining({ text: expect.stringMatching(/cancelar/i) }),
        expect.objectContaining({ text: expect.stringMatching(/sair/i) }),
      ]),
    );
  });

  it('chama logout() ao confirmar o Alert', async () => {
    mockLogout.mockResolvedValue(undefined);
    const { getByTestId } = renderScreen();
    fireEvent.press(getByTestId('logout-button'));

    const buttons = (Alert.alert as jest.Mock).mock.calls[0][2] as Array<{
      text: string;
      onPress?: () => void;
    }>;
    const confirmBtn = buttons.find((b) => /sair/i.test(b.text));

    await act(async () => {
      confirmBtn?.onPress?.();
    });

    expect(mockLogout).toHaveBeenCalledTimes(1);
  });

  it('navega para /(auth)/login após confirmar o logout', async () => {
    mockLogout.mockResolvedValue(undefined);
    const { getByTestId } = renderScreen();
    fireEvent.press(getByTestId('logout-button'));

    const buttons = (Alert.alert as jest.Mock).mock.calls[0][2] as Array<{
      text: string;
      onPress?: () => void;
    }>;
    const confirmBtn = buttons.find((b) => /sair/i.test(b.text));

    await act(async () => {
      confirmBtn?.onPress?.();
    });

    expect(mockReplace).toHaveBeenCalledWith('/(auth)/login');
  });
});
