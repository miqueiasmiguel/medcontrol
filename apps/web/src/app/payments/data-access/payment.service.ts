import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export type PaymentItemStatus = 'Pending' | 'Paid' | 'Refused';
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
  status: PaymentItemStatus;
  notes: string | null;
}

export interface PaymentDto {
  id: string;
  tenantId: string;
  doctorId: string;
  healthPlanId: string;
  executionDate: string;
  appointmentNumber: string;
  authorizationCode: string | null;
  beneficiaryCard: string;
  beneficiaryName: string;
  executionLocation: string;
  paymentLocation: string;
  notes: string | null;
  status: PaymentStatus;
  items: PaymentItemDto[];
}

export interface CreatePaymentItemRequest {
  procedureId: string;
  value: number;
}

export interface CreatePaymentCommand {
  doctorId: string;
  healthPlanId: string;
  executionDate: string;
  appointmentNumber: string;
  authorizationCode?: string;
  beneficiaryCard: string;
  beneficiaryName: string;
  executionLocation: string;
  paymentLocation: string;
  notes?: string;
  items: CreatePaymentItemRequest[];
}

export interface UpdatePaymentCommand {
  executionDate: string;
  appointmentNumber: string;
  authorizationCode?: string;
  beneficiaryCard: string;
  beneficiaryName: string;
  executionLocation: string;
  paymentLocation: string;
  notes?: string;
}

export interface UpdatePaymentItemStatusCommand {
  status: PaymentItemStatus;
  notes?: string;
}

export interface AddPaymentItemRequest {
  procedureId: string;
  value: number;
}

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private readonly http = inject(HttpClient);

  getPayments() {
    return this.http.get<PaymentDto[]>('/api/payments', { withCredentials: true });
  }

  getPayment(id: string) {
    return this.http.get<PaymentDto>(`/api/payments/${id}`, { withCredentials: true });
  }

  createPayment(command: CreatePaymentCommand) {
    return this.http.post<PaymentDto>('/api/payments', command, { withCredentials: true });
  }

  updatePayment(id: string, command: UpdatePaymentCommand) {
    return this.http.patch<PaymentDto>(`/api/payments/${id}`, command, { withCredentials: true });
  }

  updatePaymentItemStatus(
    paymentId: string,
    itemId: string,
    command: UpdatePaymentItemStatusCommand,
  ) {
    return this.http.patch<PaymentDto>(
      `/api/payments/${paymentId}/items/${itemId}`,
      command,
      { withCredentials: true },
    );
  }

  addPaymentItem(paymentId: string, request: AddPaymentItemRequest) {
    return this.http.post<PaymentDto>(
      `/api/payments/${paymentId}/items`,
      request,
      { withCredentials: true },
    );
  }

  removePaymentItem(paymentId: string, itemId: string) {
    return this.http.delete<PaymentDto>(
      `/api/payments/${paymentId}/items/${itemId}`,
      { withCredentials: true },
    );
  }
}
