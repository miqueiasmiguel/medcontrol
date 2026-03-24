import { act, renderHook } from '@testing-library/react-native';
import { PaymentService, type PaymentDto } from '../../services/payment.service';
import { usePayment } from '../usePayment';

jest.mock('../../services/payment.service');

const mockPaymentService = PaymentService as jest.Mocked<typeof PaymentService>;

const fakePayment: PaymentDto = {
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
  items: [
    { id: 'i1', procedureId: 'proc1', value: 150, status: 'Paid' },
    { id: 'i2', procedureId: 'proc2', value: 100, status: 'Paid' },
  ],
};

beforeEach(() => {
  jest.clearAllMocks();
});

describe('usePayment', () => {
  it('começa com loading true e payment null quando id é fornecido', () => {
    mockPaymentService.getPayment.mockResolvedValue(fakePayment);
    const { result } = renderHook(() => usePayment('p1'));

    expect(result.current.loading).toBe(true);
    expect(result.current.payment).toBeNull();
    expect(result.current.error).toBeNull();
  });

  it('carrega o pagamento ao montar', async () => {
    mockPaymentService.getPayment.mockResolvedValue(fakePayment);
    const { result } = renderHook(() => usePayment('p1'));

    await act(() => Promise.resolve());

    expect(result.current.loading).toBe(false);
    expect(result.current.payment).toEqual(fakePayment);
    expect(result.current.error).toBeNull();
    expect(mockPaymentService.getPayment).toHaveBeenCalledWith('p1');
  });

  it('define error quando getPayment falha', async () => {
    mockPaymentService.getPayment.mockRejectedValue(new Error('Not found'));
    const { result } = renderHook(() => usePayment('p1'));

    await act(() => Promise.resolve());

    expect(result.current.loading).toBe(false);
    expect(result.current.payment).toBeNull();
    expect(result.current.error).toBe('Not found');
  });

  it('usa mensagem genérica quando erro não é instância de Error', async () => {
    mockPaymentService.getPayment.mockRejectedValue('timeout');
    const { result } = renderHook(() => usePayment('p1'));

    await act(() => Promise.resolve());

    expect(result.current.error).toBe('Erro ao carregar pagamento');
  });

  it('refetch chama getPayment novamente', async () => {
    mockPaymentService.getPayment.mockResolvedValue(fakePayment);
    const { result } = renderHook(() => usePayment('p1'));

    await act(() => Promise.resolve());
    await act(() => result.current.refetch());

    expect(mockPaymentService.getPayment).toHaveBeenCalledTimes(2);
  });

  it('não chama getPayment quando id é undefined', () => {
    const { result } = renderHook(() => usePayment(undefined));

    expect(result.current.loading).toBe(false);
    expect(result.current.payment).toBeNull();
    expect(mockPaymentService.getPayment).not.toHaveBeenCalled();
  });
});
