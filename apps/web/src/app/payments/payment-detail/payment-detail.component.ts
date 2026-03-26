import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  EventEmitter,
  Input,
  Output,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CurrencyPipe } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  PaymentService,
  PaymentDto,
  PaymentItemStatus,
  PaymentStatus,
} from '../data-access/payment.service';
import { ProcedureDto } from '../../procedures/data-access/procedure.service';
import { CurrentUserService } from '../../core/data-access/current-user.service';

@Component({
  selector: 'app-payment-detail',
  standalone: true,
  imports: [ReactiveFormsModule, CurrencyPipe],
  templateUrl: './payment-detail.component.html',
  styleUrl: './payment-detail.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaymentDetailComponent {
  @Input({ required: true }) payment!: PaymentDto;
  @Input() doctorName = '';
  @Input() healthPlanName = '';
  @Input() procedures: ProcedureDto[] = [];
  @Output() readonly updated = new EventEmitter<PaymentDto>();
  @Output() readonly closed = new EventEmitter<void>();

  private readonly paymentService = inject(PaymentService);
  private readonly currentUserService = inject(CurrentUserService);
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly isDoctor = this.currentUserService.isDoctor;

  // Item status editing
  readonly editingItemId = signal<string | null>(null);
  readonly updatingItemId = signal<string | null>(null);

  // Header editing
  readonly editingHeader = signal(false);
  readonly headerSubmitting = signal(false);

  // Add item
  readonly addingItem = signal(false);
  readonly itemSubmitting = signal(false);

  // Remove item
  readonly removingItemId = signal<string | null>(null);

  readonly errorMessage = signal('');

  readonly statusForm = this.fb.nonNullable.group({
    status: ['Pending' as PaymentItemStatus, Validators.required],
    notes: [''],
  });

  readonly headerForm = this.fb.group({
    executionDate: ['', Validators.required],
    appointmentNumber: ['', [Validators.required, Validators.maxLength(100)]],
    authorizationCode: [''],
    beneficiaryCard: ['', [Validators.required, Validators.maxLength(50)]],
    beneficiaryName: ['', [Validators.required, Validators.maxLength(256)]],
    executionLocation: ['', [Validators.required, Validators.maxLength(256)]],
    paymentLocation: ['', [Validators.required, Validators.maxLength(256)]],
    notes: [''],
  });

  readonly addItemForm = this.fb.group({
    procedureId: ['', Validators.required],
    value: [0, [Validators.required, Validators.min(0.01)]],
  });

  // ── Item status ──────────────────────────────────────────────────────────

  startEdit(itemId: string, currentStatus: PaymentItemStatus, currentNotes: string | null) {
    this.editingItemId.set(itemId);
    this.statusForm.patchValue({ status: currentStatus, notes: currentNotes ?? '' });
    this.errorMessage.set('');
  }

  cancelEdit() {
    this.editingItemId.set(null);
  }

  submitStatusUpdate(itemId: string) {
    if (this.statusForm.invalid) return;

    this.updatingItemId.set(itemId);
    this.errorMessage.set('');

    const raw = this.statusForm.getRawValue();

    this.paymentService
      .updatePaymentItemStatus(this.payment.id, itemId, {
        status: raw.status,
        notes: raw.notes || undefined,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (payment) => {
          this.updatingItemId.set(null);
          this.editingItemId.set(null);
          this.updated.emit(payment);
        },
        error: () => {
          this.updatingItemId.set(null);
          this.errorMessage.set('Erro ao atualizar status. Tente novamente.');
        },
      });
  }

  // ── Header editing ───────────────────────────────────────────────────────

  startEditHeader() {
    this.headerForm.patchValue({
      executionDate: this.payment.executionDate,
      appointmentNumber: this.payment.appointmentNumber,
      authorizationCode: this.payment.authorizationCode ?? '',
      beneficiaryCard: this.payment.beneficiaryCard,
      beneficiaryName: this.payment.beneficiaryName,
      executionLocation: this.payment.executionLocation,
      paymentLocation: this.payment.paymentLocation,
      notes: this.payment.notes ?? '',
    });
    this.editingHeader.set(true);
    this.errorMessage.set('');
  }

  cancelEditHeader() {
    this.editingHeader.set(false);
  }

  submitHeaderUpdate() {
    if (this.headerForm.invalid) {
      this.headerForm.markAllAsTouched();
      return;
    }

    this.headerSubmitting.set(true);
    this.errorMessage.set('');

    const raw = this.headerForm.getRawValue();

    this.paymentService
      .updatePayment(this.payment.id, {
        executionDate: raw.executionDate ?? '',
        appointmentNumber: raw.appointmentNumber ?? '',
        authorizationCode: raw.authorizationCode || undefined,
        beneficiaryCard: raw.beneficiaryCard ?? '',
        beneficiaryName: raw.beneficiaryName ?? '',
        executionLocation: raw.executionLocation ?? '',
        paymentLocation: raw.paymentLocation ?? '',
        notes: raw.notes || undefined,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (payment) => {
          this.headerSubmitting.set(false);
          this.editingHeader.set(false);
          this.updated.emit(payment);
        },
        error: () => {
          this.headerSubmitting.set(false);
          this.errorMessage.set('Erro ao atualizar pagamento. Tente novamente.');
        },
      });
  }

  // ── Add item ─────────────────────────────────────────────────────────────

  startAddItem() {
    this.addItemForm.reset({ procedureId: '', value: 0 });
    this.addingItem.set(true);
    this.errorMessage.set('');
  }

  cancelAddItem() {
    this.addingItem.set(false);
  }

  onAddProcedureSelected() {
    const procedureId = this.addItemForm.get('procedureId')?.value as string;
    const procedure = this.procedures.find((p) => p.id === procedureId);
    if (procedure) {
      this.addItemForm.patchValue({ value: procedure.value });
    }
  }

  submitAddItem() {
    if (this.addItemForm.invalid) {
      this.addItemForm.markAllAsTouched();
      return;
    }

    this.itemSubmitting.set(true);
    this.errorMessage.set('');

    const raw = this.addItemForm.getRawValue();

    this.paymentService
      .addPaymentItem(this.payment.id, {
        procedureId: raw.procedureId ?? '',
        value: raw.value ?? 0,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (payment) => {
          this.itemSubmitting.set(false);
          this.addingItem.set(false);
          this.updated.emit(payment);
        },
        error: () => {
          this.itemSubmitting.set(false);
          this.errorMessage.set('Erro ao adicionar procedimento. Tente novamente.');
        },
      });
  }

  // ── Remove item ──────────────────────────────────────────────────────────

  removeItem(itemId: string) {
    this.removingItemId.set(itemId);
    this.errorMessage.set('');

    this.paymentService
      .removePaymentItem(this.payment.id, itemId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (payment) => {
          this.removingItemId.set(null);
          this.updated.emit(payment);
        },
        error: () => {
          this.removingItemId.set(null);
          this.errorMessage.set('Erro ao remover procedimento. Tente novamente.');
        },
      });
  }

  // ── Helpers ──────────────────────────────────────────────────────────────

  procedureName(procedureId: string): string {
    const found = this.procedures.find((p) => p.id === procedureId);
    return found ? `${found.code} — ${found.description}` : procedureId.slice(0, 8) + '…';
  }

  itemStatusLabel(status: PaymentItemStatus): string {
    if (status === 'Paid') return 'Pago';
    if (status === 'Refused') return 'Recusado';
    return 'Pendente';
  }

  statusBadgeClass(status: PaymentStatus): string {
    if (status === 'PartiallyPending') return 'badge-pending';
    if (status === 'PartiallyRefused') return 'badge-refused';
    return `badge-${status.toLowerCase()}`;
  }

  statusLabel(status: PaymentStatus): string {
    const labels: Record<PaymentStatus, string> = {
      Pending: 'Pendente',
      Paid: 'Pago',
      Refused: 'Recusado',
      PartiallyPending: 'Parc. pendente',
      PartiallyRefused: 'Parc. recusado',
    };
    return labels[status];
  }

  totalValue(): number {
    return this.payment.items.reduce((sum, item) => sum + item.value, 0);
  }

  formatDate(dateStr: string): string {
    const [year, month, day] = dateStr.split('-');
    return `${day}/${month}/${year}`;
  }

  close() {
    this.closed.emit();
  }
}
