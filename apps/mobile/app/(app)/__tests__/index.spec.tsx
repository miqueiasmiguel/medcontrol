import React from 'react';
import { Alert } from 'react-native';
import { render, fireEvent, act } from '@testing-library/react-native';
import HomeScreen from '../index';
import { useAuth } from '../../../src/hooks/useAuth';
import { useCurrentUser } from '../../../src/hooks/useCurrentUser';
import { usePayments } from '../../../src/hooks/usePayments';

// ── Mocks ────────────────────────────────────────────────────────────────────

jest.mock('@react-native-async-storage/async-storage', () =>
  require('@react-native-async-storage/async-storage/jest/async-storage-mock'),
);

jest.mock('../../../src/hooks/useAuth');
jest.mock('../../../src/hooks/useCurrentUser');
jest.mock('../../../src/hooks/usePayments');

jest.mock('../../../src/services/health-plan.service', () => ({
  HealthPlanService: {
    listHealthPlans: jest.fn().mockResolvedValue([]),
  },
}));

jest.mock('@medcontrol/design-system/native', () => ({
  useTheme: () => ({
    colors: {
      primary: '#0EA5E9',
      primaryLight: '#E0F2FE',
      secondary: '#0F172A',
      secondaryText: '#94A3B8',
      success: { base: '#22C55E' },
      warning: { base: '#F59E0B' },
      error: { base: '#EF4444' },
      border: '#E2E8F0',
      borderStrong: '#94A3B8',
      divider: '#F1F5F9',
      text: {
        primary: '#0F172A',
        secondary: '#64748B',
        tertiary: '#94A3B8',
        onDark: '#FFFFFF',
        onDarkSubtle: 'rgba(255,255,255,0.7)',
      },
      surface: {
        background: '#F8FAFC',
        card: '#FFFFFF',
        cardPressed: '#F1F5F9',
        overlay: 'rgba(0,0,0,0.5)',
      },
      paymentStatus: {
        pending: { bg: '#FEF3C7', border: '#FCD34D', dot: '#F59E0B', text: '#92400E' },
        paid: { bg: '#DCFCE7', border: '#86EFAC', dot: '#22C55E', text: '#166534' },
        refused: { bg: '#FEE2E2', border: '#FCA5A5', dot: '#EF4444', text: '#991B1B' },
      },
    },
    spacing: new Proxy({}, { get: (_t, p) => Number(p) * 4 }),
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

jest.mock('react-native-paper', () => ({
  Text: ({ children, ...props }: { children: React.ReactNode; [key: string]: unknown }) => {
    const { Text: RNText } = jest.requireActual('react-native');
    return <RNText {...props}>{children}</RNText>;
  },
}));

jest.mock('@expo/vector-icons', () => ({
  Ionicons: () => null,
}));

jest.mock('expo-constants', () => ({
  default: { expoConfig: { extra: { apiUrl: 'http://localhost:5000' } } },
}));

const mockReplace = jest.fn();
const mockPush = jest.fn();
jest.mock('expo-router', () => ({
  useRouter: () => ({ replace: mockReplace, push: mockPush }),
}));

// ── Helpers ───────────────────────────────────────────────────────────────────

const mockLogout = jest.fn();

function setupPayments(overrides?: Partial<ReturnType<typeof usePayments>>) {
  jest.mocked(usePayments).mockReturnValue({
    payments: [],
    loading: false,
    error: null,
    refetch: jest.fn(),
    ...overrides,
  });
}

function setupAuth(overrides?: Partial<ReturnType<typeof useAuth>>) {
  jest.mocked(useAuth).mockReturnValue({
    isAuthenticated: true,
    isLoading: false,
    logout: mockLogout,
    setSession: jest.fn(),
    ...overrides,
  });
}

function setupUser(overrides?: Partial<ReturnType<typeof useCurrentUser>>) {
  jest.mocked(useCurrentUser).mockReturnValue({
    user: null,
    loading: false,
    error: null,
    ...overrides,
  });
}

beforeEach(() => {
  jest.clearAllMocks();
  setupAuth();
  setupUser();
  setupPayments();
  jest.spyOn(Alert, 'alert').mockImplementation(() => undefined);
  mockReplace.mockReset();
  mockPush.mockReset();
});

// ── Testes ────────────────────────────────────────────────────────────────────

describe('HomeScreen — logout', () => {
  it('renderiza o botão de logout', () => {
    const { getByTestId } = render(<HomeScreen />);
    expect(getByTestId('logout-button')).toBeTruthy();
  });

  it('exibe Alert de confirmação ao pressionar o botão de logout', () => {
    const { getByTestId } = render(<HomeScreen />);
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
    const { getByTestId } = render(<HomeScreen />);
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
    const { getByTestId } = render(<HomeScreen />);
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

describe('HomeScreen — saudação personalizada', () => {
  it('exibe o primeiro nome do displayName', () => {
    setupUser({
      user: {
        id: 'u1',
        email: 'joao@clinica.com',
        displayName: 'João Silva',
        isEmailVerified: true,
        globalRole: 'user',
      },
    });
    const { getByTestId } = render(<HomeScreen />);
    expect(getByTestId('hero-greeting')).toHaveTextContent('Olá, João');
  });

  it('usa email como fallback quando displayName não está definido', () => {
    setupUser({
      user: {
        id: 'u2',
        email: 'maria@clinica.com',
        isEmailVerified: true,
        globalRole: 'user',
      },
    });
    const { getByTestId } = render(<HomeScreen />);
    expect(getByTestId('hero-greeting')).toHaveTextContent('Olá, maria@clinica.com');
  });

  it('exibe saudação genérica enquanto carrega', () => {
    setupUser({ user: null, loading: true });
    const { getByTestId } = render(<HomeScreen />);
    expect(getByTestId('hero-greeting')).toHaveTextContent('Olá');
  });

  it('exibe Image quando avatarUrl está presente', () => {
    setupUser({
      user: {
        id: 'u1',
        email: 'joao@clinica.com',
        avatarUrl: 'https://cdn.example.com/a.jpg',
        isEmailVerified: true,
        globalRole: 'user',
      },
    });
    const { getByTestId } = render(<HomeScreen />);
    expect(getByTestId('hero-avatar-image')).toBeTruthy();
  });

  it('exibe ícone genérico quando avatarUrl está ausente', () => {
    setupUser({
      user: {
        id: 'u1',
        email: 'joao@clinica.com',
        isEmailVerified: true,
        globalRole: 'user',
      },
    });
    const { getByTestId } = render(<HomeScreen />);
    expect(getByTestId('hero-avatar-icon')).toBeTruthy();
  });
});

describe('HomeScreen — botão de settings', () => {
  it('renderiza o botão de settings', () => {
    const { getByTestId } = render(<HomeScreen />);
    expect(getByTestId('settings-button')).toBeTruthy();
  });

  it('navega para /settings ao pressionar o botão de settings', () => {
    const { getByTestId } = render(<HomeScreen />);
    fireEvent.press(getByTestId('settings-button'));
    expect(mockPush).toHaveBeenCalledWith('/settings');
  });
});

describe('HomeScreen — navegação para detalhe', () => {
  it('navega para /payments/{id} ao pressionar um PaymentCard', () => {
    setupPayments({
      payments: [
        {
          id: 'p1',
          tenantId: 't1',
          doctorId: 'd1',
          healthPlanId: 'hp1',
          executionDate: '2025-01-15',
          appointmentNumber: 'ATD-001',
          beneficiaryCard: '123456789',
          beneficiaryName: 'Maria Aparecida',
          executionLocation: 'Hospital São Lucas',
          paymentLocation: 'Financeiro',
          status: 'Paid',
          totalValue: 250,
          items: [],
        },
      ],
    });
    const { getByTestId } = render(<HomeScreen />);
    fireEvent.press(getByTestId('payment-card-p1'));
    expect(mockPush).toHaveBeenCalledWith({ pathname: '/payments/[id]', params: { id: 'p1' } });
  });
});
