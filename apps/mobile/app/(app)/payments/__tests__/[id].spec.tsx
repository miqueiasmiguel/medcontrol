import React from 'react';
import { render, fireEvent, act } from '@testing-library/react-native';
import PaymentDetailScreen from '../[id]';
import { usePayment } from '../../../../src/hooks/usePayment';

// ── Mocks ────────────────────────────────────────────────────────────────────

jest.mock('@react-native-async-storage/async-storage', () =>
  require('@react-native-async-storage/async-storage/jest/async-storage-mock'),
);

jest.mock('../../../../src/hooks/usePayment');

jest.mock('../../../../src/services/health-plan.service', () => ({
  HealthPlanService: {
    listHealthPlans: jest.fn().mockResolvedValue([
      { id: 'hp1', name: 'Unimed', tissCode: '001', tenantId: 't1' },
    ]),
  },
}));

const mockTheme = {
  colors: {
    primary: '#F97316',
    primaryText: '#FFFFFF',
    primaryLight: '#FFF4ED',
    secondary: '#1B2E63',
    success: { base: '#10B981', light: '#ECFDF5', dark: '#065F46' },
    warning: { base: '#F59E0B', light: '#FFFBEB', dark: '#92400E' },
    error: { base: '#EF4444', light: '#FEF2F2', dark: '#991B1B' },
    border: '#E9ECEF',
    borderStrong: '#DEE2E6',
    divider: '#F1F3F5',
    text: {
      primary: '#212529',
      secondary: '#868E96',
      tertiary: '#ADB5BD',
      disabled: '#CED4DA',
      inverse: '#FFFFFF',
      onDark: 'rgba(255, 255, 255, 0.87)',
      onDarkSubtle: 'rgba(255, 255, 255, 0.55)',
      link: '#EA6310',
    },
    surface: {
      background: '#F8F9FA',
      card: '#FFFFFF',
      cardPressed: '#F1F3F5',
      overlay: 'rgba(15, 26, 64, 0.48)',
      nav: '#0F1A40',
      navActive: '#1B2E63',
    },
    paymentStatus: {
      pending: { bg: '#FFFBEB', border: '#FDE68A', dot: '#F59E0B', text: '#92400E' },
      paid: { bg: '#ECFDF5', border: '#A7F3D0', dot: '#10B981', text: '#065F46' },
      refused: { bg: '#FEF2F2', border: '#FECACA', dot: '#EF4444', text: '#991B1B' },
    },
  },
  spacing: new Proxy({}, { get: (_t: object, p: string | symbol) => Number(p) * 4 }),
  borderRadius: new Proxy({}, { get: () => 8 }),
  typography: {
    fontSize: { xs: 12, sm: 14, md: 16, lg: 18, xl: 20, '2xl': 24, '3xl': 30 },
    fontWeight: { regular: '400', medium: '500', semibold: '600', bold: '700' },
  },
  shadows: { sm: {}, md: {} },
  components: { buttonHeight: 44, buttonHeightSm: 36, avatarMd: 40, iconSm: 16, iconMd: 20, iconLg: 24, iconXl: 32 },
};

jest.mock('@medcontrol/design-system/native', () => ({
  useTheme: () => mockTheme,
  theme: mockTheme,
  darkTheme: mockTheme,
}));

jest.mock('../../../../src/contexts/ThemeContext', () => ({
  useAppTheme: () => mockTheme,
  useThemePreference: () => ({ preference: 'system', setPreference: jest.fn() }),
  ThemePreferenceProvider: ({ children }: { children: React.ReactNode }) => children,
}));

jest.mock('react-native-safe-area-context', () => ({
  useSafeAreaInsets: () => ({ top: 0, bottom: 0, left: 0, right: 0 }),
}));

jest.mock('react-native-paper', () => ({
  Text: ({ children, ...props }: { children: React.ReactNode; [key: string]: unknown }) => {
    const { Text: RNText } = jest.requireActual('react-native');
    return <RNText {...props}>{children}</RNText>;
  },
  ActivityIndicator: ({ ...props }: { [key: string]: unknown }) => {
    const { View } = jest.requireActual('react-native');
    return <View testID="activity-indicator" {...props} />;
  },
}));

jest.mock('@expo/vector-icons', () => ({
  Ionicons: () => null,
}));

jest.mock('expo-constants', () => ({
  default: { expoConfig: { extra: { apiUrl: 'http://localhost:5000' } } },
}));

const mockBack = jest.fn();
jest.mock('expo-router', () => ({
  useRouter: () => ({ back: mockBack }),
  useLocalSearchParams: () => ({ id: 'p1' }),
}));

// ── Fixtures ──────────────────────────────────────────────────────────────────

const fakePayment = {
  id: 'p1',
  tenantId: 't1',
  doctorId: 'd1',
  healthPlanId: 'hp1',
  executionDate: '2025-01-15',
  appointmentNumber: 'ATD-001',
  authorizationCode: 'AUTH-XYZ',
  beneficiaryCard: '123456789',
  beneficiaryName: 'Maria Aparecida',
  executionLocation: 'Hospital São Lucas',
  paymentLocation: 'Financeiro',
  notes: 'Observação de teste',
  status: 'Paid' as const,
  totalValue: 250,
  items: [
    { id: 'i1', procedureId: 'proc1', value: 150, status: 'Paid' as const },
    { id: 'i2', procedureId: 'proc2', value: 100, status: 'Paid' as const, notes: 'nota item' },
  ],
};

const mockRefetch = jest.fn();

function setupPayment(
  overrides: Partial<ReturnType<typeof usePayment>> = {},
) {
  jest.mocked(usePayment).mockReturnValue({
    payment: fakePayment,
    loading: false,
    error: null,
    refetch: mockRefetch,
    ...overrides,
  });
}

beforeEach(() => {
  jest.clearAllMocks();
  mockBack.mockReset();
});

// ── Testes ────────────────────────────────────────────────────────────────────

describe('PaymentDetailScreen — loading', () => {
  it('exibe indicador de carregamento quando loading é true', () => {
    setupPayment({ payment: null, loading: true });
    const { getByTestId } = render(<PaymentDetailScreen />);
    expect(getByTestId('loading-indicator')).toBeTruthy();
  });
});

describe('PaymentDetailScreen — error', () => {
  it('exibe mensagem de erro quando error não é null', () => {
    setupPayment({ payment: null, error: 'HTTP 404' });
    const { getByTestId } = render(<PaymentDetailScreen />);
    expect(getByTestId('error-message')).toBeTruthy();
    expect(getByTestId('retry-button')).toBeTruthy();
  });

  it('chama refetch ao pressionar o botão de nova tentativa', () => {
    setupPayment({ payment: null, error: 'HTTP 404', refetch: mockRefetch });
    const { getByTestId } = render(<PaymentDetailScreen />);
    fireEvent.press(getByTestId('retry-button'));
    expect(mockRefetch).toHaveBeenCalledTimes(1);
  });
});

describe('PaymentDetailScreen — conteúdo', () => {
  it('exibe o nome do beneficiário', () => {
    setupPayment();
    const { getByTestId } = render(<PaymentDetailScreen />);
    expect(getByTestId('beneficiary-name')).toHaveTextContent('Maria Aparecida');
  });

  it('exibe o badge de status com label correto para Paid', () => {
    setupPayment();
    const { getByTestId } = render(<PaymentDetailScreen />);
    expect(getByTestId('status-badge')).toHaveTextContent('Pago');
  });

  it('exibe a data de execução formatada em pt-BR', () => {
    setupPayment();
    const { getByTestId } = render(<PaymentDetailScreen />);
    expect(getByTestId('execution-date')).toHaveTextContent('15');
    expect(getByTestId('execution-date')).toHaveTextContent('2025');
  });

  it('exibe o número do atendimento', () => {
    setupPayment();
    const { getByTestId } = render(<PaymentDetailScreen />);
    expect(getByTestId('appointment-number')).toHaveTextContent('ATD-001');
  });

  it('exibe o código de autorização quando presente', () => {
    setupPayment();
    const { getByTestId } = render(<PaymentDetailScreen />);
    expect(getByTestId('authorization-code')).toHaveTextContent('AUTH-XYZ');
  });

  it('não exibe o código de autorização quando ausente', () => {
    setupPayment({
      payment: { ...fakePayment, authorizationCode: undefined },
    });
    const { queryByTestId } = render(<PaymentDetailScreen />);
    expect(queryByTestId('authorization-code')).toBeNull();
  });

  it('exibe o total formatado como moeda brasileira', () => {
    setupPayment();
    const { getByTestId } = render(<PaymentDetailScreen />);
    expect(getByTestId('total-value')).toHaveTextContent('250');
  });

  it('renderiza cada item da lista', () => {
    setupPayment();
    const { getAllByTestId } = render(<PaymentDetailScreen />);
    expect(getAllByTestId('payment-item')).toHaveLength(2);
  });

  it('exibe observações quando presentes', () => {
    setupPayment();
    const { getByTestId } = render(<PaymentDetailScreen />);
    expect(getByTestId('payment-notes')).toHaveTextContent('Observação de teste');
  });

  it('não exibe observações quando ausentes', () => {
    setupPayment({ payment: { ...fakePayment, notes: undefined } });
    const { queryByTestId } = render(<PaymentDetailScreen />);
    expect(queryByTestId('payment-notes')).toBeNull();
  });

  it('exibe o local de execução', () => {
    setupPayment();
    const { getByTestId } = render(<PaymentDetailScreen />);
    expect(getByTestId('execution-location')).toHaveTextContent('Hospital São Lucas');
  });

  it('exibe o local de pagamento', () => {
    setupPayment();
    const { getByTestId } = render(<PaymentDetailScreen />);
    expect(getByTestId('payment-location')).toHaveTextContent('Financeiro');
  });

  it('exibe o nome do convênio resolvido pelo healthPlanId', async () => {
    setupPayment();
    const { getByTestId } = render(<PaymentDetailScreen />);
    await act(async () => {
      await Promise.resolve();
    });
    expect(getByTestId('health-plan-name')).toHaveTextContent('Unimed');
  });
});

describe('PaymentDetailScreen — navegação', () => {
  it('chama router.back() ao pressionar o botão de voltar', () => {
    setupPayment();
    const { getByTestId } = render(<PaymentDetailScreen />);
    fireEvent.press(getByTestId('back-button'));
    expect(mockBack).toHaveBeenCalledTimes(1);
  });
});

describe('PaymentDetailScreen — status badge variants', () => {
  it('exibe label "Pendente" para status Pending', () => {
    setupPayment({ payment: { ...fakePayment, status: 'Pending' } });
    const { getByTestId } = render(<PaymentDetailScreen />);
    expect(getByTestId('status-badge')).toHaveTextContent('Pendente');
  });

  it('exibe label "Recusado" para status Refused', () => {
    setupPayment({ payment: { ...fakePayment, status: 'Refused' } });
    const { getByTestId } = render(<PaymentDetailScreen />);
    expect(getByTestId('status-badge')).toHaveTextContent('Recusado');
  });

  it('exibe label "Parcial" para status PartiallyPending', () => {
    setupPayment({ payment: { ...fakePayment, status: 'PartiallyPending' } });
    const { getByTestId } = render(<PaymentDetailScreen />);
    expect(getByTestId('status-badge')).toHaveTextContent('Parcial');
  });

  it('exibe label "Glosa parcial" para status PartiallyRefused', () => {
    setupPayment({ payment: { ...fakePayment, status: 'PartiallyRefused' } });
    const { getByTestId } = render(<PaymentDetailScreen />);
    expect(getByTestId('status-badge')).toHaveTextContent('Glosa parcial');
  });
});
