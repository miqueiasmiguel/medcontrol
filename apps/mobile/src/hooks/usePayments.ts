import { useCallback, useEffect, useState } from 'react';
import { type PaymentDto, PaymentService } from '../services/payment.service';

export interface UsePaymentsResult {
  payments: PaymentDto[];
  loading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
}

export function usePayments(): UsePaymentsResult {
  const [payments, setPayments] = useState<PaymentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchPayments = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await PaymentService.listPayments();
      setPayments(data);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Erro ao carregar pagamentos');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void fetchPayments();
  }, [fetchPayments]);

  return { payments, loading, error, refetch: fetchPayments };
}
