import React from 'react';
import { render, fireEvent, act, waitFor } from '@testing-library/react-native';
import { Alert } from 'react-native';
import EditProfileScreen from '../EditProfileScreen';
import { useCurrentUser } from '../../../hooks/useCurrentUser';
import { useDoctorProfile } from '../../../hooks/useDoctorProfile';
import { UserService } from '../../../services/user.service';

// ── Mocks ────────────────────────────────────────────────────────────────────

jest.mock('@react-native-async-storage/async-storage', () =>
  require('@react-native-async-storage/async-storage/jest/async-storage-mock'),
);

jest.mock('../../../hooks/useCurrentUser');
jest.mock('../../../hooks/useDoctorProfile');
jest.mock('../../../services/user.service');

jest.mock('@medcontrol/design-system/native', () => ({
  useTheme: () => ({
    colors: {
      primary: '#0EA5E9',
      primaryLight: '#E0F2FE',
      secondary: '#0F172A',
      error: { base: '#EF4444' },
      border: '#E2E8F0',
      text: {
        primary: '#0F172A',
        secondary: '#64748B',
        tertiary: '#94A3B8',
        onDark: '#FFFFFF',
      },
      surface: { background: '#F8FAFC', card: '#FFFFFF' },
    },
    spacing: new Proxy({}, { get: (_t: object, p: string | symbol) => Number(p) * 4 }),
    borderRadius: new Proxy({}, { get: () => 8 }),
    typography: {
      fontSize: { xs: 12, sm: 14, md: 16, lg: 18, xl: 20 },
      fontWeight: { regular: '400', medium: '500', semibold: '600', bold: '700' },
    },
    shadows: { sm: {} },
    components: { avatarMd: 40, inputHeight: 48 },
  }),
}));

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

const mockBack = jest.fn();
jest.mock('expo-router', () => ({
  useRouter: () => ({ back: mockBack }),
}));

// ── Helpers ───────────────────────────────────────────────────────────────────

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

function setupCurrentUser(overrides?: Partial<ReturnType<typeof useCurrentUser>>) {
  jest.mocked(useCurrentUser).mockReturnValue({
    user: fakeUser,
    loading: false,
    error: null,
    ...overrides,
  });
}

function setupDoctorProfile(overrides?: Partial<ReturnType<typeof useDoctorProfile>>) {
  jest.mocked(useDoctorProfile).mockReturnValue({
    doctorProfile: fakeProfile,
    loading: false,
    error: null,
    refetch: jest.fn(),
    ...overrides,
  });
}

beforeEach(() => {
  jest.clearAllMocks();
  setupCurrentUser();
  setupDoctorProfile();
  jest.spyOn(Alert, 'alert').mockImplementation(() => undefined);
  mockUserService.updateProfile.mockResolvedValue(fakeUser);
  mockUserService.updateMyDoctorProfile.mockResolvedValue([fakeProfile]);
});

// ── Tests ─────────────────────────────────────────────────────────────────────

describe('EditProfileScreen — renderização', () => {
  it('renderiza campos preenchidos com dados do usuário e perfil médico', () => {
    const { getByTestId } = render(<EditProfileScreen />);

    expect(getByTestId('field-displayName')).toBeTruthy();
    expect(getByTestId('field-name')).toBeTruthy();
    expect(getByTestId('field-crm')).toBeTruthy();
    expect(getByTestId('field-councilState')).toBeTruthy();
    expect(getByTestId('field-specialty')).toBeTruthy();
  });
});

describe('EditProfileScreen — submit', () => {
  it('chama updateProfile e updateMyDoctorProfile ao submeter', async () => {
    const { getByTestId } = render(<EditProfileScreen />);

    await act(async () => {
      fireEvent.press(getByTestId('submit-button'));
    });

    await waitFor(() => {
      expect(mockUserService.updateProfile).toHaveBeenCalledTimes(1);
      expect(mockUserService.updateMyDoctorProfile).toHaveBeenCalledTimes(1);
    });
  });

  it('exibe loading no botão durante submit', async () => {
    // eslint-disable-next-line @typescript-eslint/no-empty-function
    let resolveUpdate: () => void = () => {};
    mockUserService.updateProfile.mockReturnValueOnce(
      new Promise((res) => {
        resolveUpdate = () => res(fakeUser);
      }),
    );
    mockUserService.updateMyDoctorProfile.mockResolvedValue([fakeProfile]);

    const { getByTestId } = render(<EditProfileScreen />);

    act(() => {
      fireEvent.press(getByTestId('submit-button'));
    });

    expect(getByTestId('submit-button')).toBeTruthy();

    await act(async () => {
      resolveUpdate();
    });
  });

  it('exibe alerta de sucesso após salvar', async () => {
    const { getByTestId } = render(<EditProfileScreen />);

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

  it('exibe alerta de erro quando update falha', async () => {
    mockUserService.updateProfile.mockRejectedValueOnce(new Error('Erro de conexão'));

    const { getByTestId } = render(<EditProfileScreen />);

    await act(async () => {
      fireEvent.press(getByTestId('submit-button'));
    });

    await waitFor(() => {
      expect(Alert.alert).toHaveBeenCalledWith(
        expect.stringMatching(/erro/i),
        expect.stringContaining('Erro de conexão'),
      );
    });
  });
});

describe('EditProfileScreen — validação', () => {
  it('não submete quando crm está vazio', async () => {
    const { getByTestId } = render(<EditProfileScreen />);

    fireEvent.changeText(getByTestId('field-crm'), '');

    await act(async () => {
      fireEvent.press(getByTestId('submit-button'));
    });

    expect(mockUserService.updateMyDoctorProfile).not.toHaveBeenCalled();
  });
});
