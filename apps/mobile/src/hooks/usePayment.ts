import { useCallback, useEffect, useState } from 'react';
import { type PaymentDto, PaymentService } from '../services/payment.service';

export interface UsePaymentResult {
  payment: PaymentDto | null;
  loading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
}

export function usePayment(id: string | undefined): UsePaymentResult {
  const [payment, setPayment] = useState<PaymentDto | null>(null);
  const [loading, setLoading] = useState(id != null);
  const [error, setError] = useState<string | null>(null);

  const fetchPayment = useCallback(async () => {
    if (!id) return;
    try {
      setLoading(true);
      setError(null);
      setPayment(await PaymentService.getPayment(id));
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Erro ao carregar pagamento');
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    void fetchPayment();
  }, [fetchPayment]);

  return { payment, loading, error, refetch: fetchPayment };
}
