import { PaymentService, type PaymentDto } from '../payment.service';

const mockFetch = jest.fn();
global.fetch = mockFetch;

beforeEach(() => {
  mockFetch.mockReset();
});

const fakePayment: PaymentDto = {
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
  status: 'Paid',
  totalValue: 250,
  items: [
    { id: 'i1', procedureId: 'proc1', value: 150, status: 'Paid' },
    { id: 'i2', procedureId: 'proc2', value: 100, status: 'Paid', notes: 'nota item' },
  ],
};

describe('PaymentService', () => {
  describe('getPayment', () => {
    it('faz GET para /payments/{id} com credentials include', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 200,
        json: async () => fakePayment,
      });

      const result = await PaymentService.getPayment('p1');

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/payments/p1'),
        expect.objectContaining({ credentials: 'include' }),
      );
      expect(result.id).toBe('p1');
    });

    it('retorna PaymentDto completo com items', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 200,
        json: async () => fakePayment,
      });

      const result = await PaymentService.getPayment('p1');

      expect(result.items).toHaveLength(2);
      expect(result.totalValue).toBe(250);
      expect(result.beneficiaryName).toBe('Maria Aparecida');
    });

    it('lança erro HTTP 404 quando pagamento não existe', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 404,
        json: async () => ({ message: 'Not found' }),
      });

      await expect(PaymentService.getPayment('missing')).rejects.toThrow('Not found');
    });

    it('lança erro HTTP genérico quando body não tem message', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 500,
        json: async () => ({}),
      });

      await expect(PaymentService.getPayment('p1')).rejects.toThrow('HTTP 500');
    });

    it('lança erro de rede quando fetch rejeita', async () => {
      mockFetch.mockRejectedValueOnce(new Error('Network request failed'));

      await expect(PaymentService.getPayment('p1')).rejects.toThrow('Network request failed');
    });
  });
});
