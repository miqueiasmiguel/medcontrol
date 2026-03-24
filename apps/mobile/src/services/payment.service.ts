import Constants from 'expo-constants';

const API_BASE: string =
  (Constants.expoConfig?.extra?.apiUrl as string | undefined) ?? 'http://localhost:5000';

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, {
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    ...options,
  });

  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    throw new Error((body as { message?: string }).message ?? `HTTP ${res.status}`);
  }

  return res.json() as Promise<T>;
}

export type PaymentStatus =
  | 'Pending'
  | 'Paid'
  | 'Refused'
  | 'PartiallyPending'
  | 'PartiallyRefused';

export interface PaymentItemDto {
  id: string;
  procedureId: string;
  value: number;
  status: 'Pending' | 'Paid' | 'Refused';
  notes?: string;
}

export interface PaymentDto {
  id: string;
  tenantId: string;
  doctorId: string;
  healthPlanId: string;
  executionDate: string; // "YYYY-MM-DD"
  appointmentNumber: string;
  authorizationCode?: string;
  beneficiaryCard: string;
  beneficiaryName: string;
  executionLocation: string;
  paymentLocation: string;
  notes?: string;
  status: PaymentStatus;
  totalValue: number;
  items: PaymentItemDto[];
}

export interface ListPaymentsParams {
  doctorId?: string;
  healthPlanId?: string;
  status?: string;
  dateFrom?: string;
  dateTo?: string;
  search?: string;
  sortBy?: 'ExecutionDate' | 'TotalValue';
  sortDescending?: boolean;
}

export const PaymentService = {
  listPayments: async (params?: ListPaymentsParams): Promise<PaymentDto[]> => {
    const entries = Object.entries(params ?? {}).filter(([, v]) => v != null) as [
      string,
      string,
    ][];
    const qs = entries.length > 0 ? '?' + new URLSearchParams(entries).toString() : '';
    return request<PaymentDto[]>(`/payments${qs}`);
  },
};
