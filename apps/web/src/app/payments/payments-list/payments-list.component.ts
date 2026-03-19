import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { PaymentService, PaymentDto, PaymentStatus } from '../data-access/payment.service';
import { PaymentFormComponent } from '../payment-form/payment-form.component';
import { PaymentDetailComponent } from '../payment-detail/payment-detail.component';
import { DoctorService, DoctorDto } from '../../doctors/data-access/doctor.service';
import { HealthPlanService, HealthPlanDto } from '../../health-plans/data-access/health-plan.service';
import { ProcedureService, ProcedureDto } from '../../procedures/data-access/procedure.service';

@Component({
  selector: 'app-payments-list',
  standalone: true,
  imports: [PaymentFormComponent, PaymentDetailComponent, CurrencyPipe],
  templateUrl: './payments-list.component.html',
  styleUrl: './payments-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaymentsListComponent implements OnInit {
  private readonly paymentService = inject(PaymentService);
  private readonly doctorService = inject(DoctorService);
  private readonly healthPlanService = inject(HealthPlanService);
  private readonly procedureService = inject(ProcedureService);
  private readonly destroyRef = inject(DestroyRef);

  readonly payments = signal<PaymentDto[]>([]);
  readonly doctors = signal<DoctorDto[]>([]);
  readonly healthPlans = signal<HealthPlanDto[]>([]);
  readonly procedures = signal<ProcedureDto[]>([]);
  readonly formOpen = signal(false);
  readonly detailOpen = signal(false);
  readonly selectedPayment = signal<PaymentDto | null>(null);
  readonly errorMessage = signal('');
  readonly statusFilter = signal<PaymentStatus | 'All'>('All');

  readonly filteredPayments = computed(() => {
    const filter = this.statusFilter();
    const list = this.payments();
    if (filter === 'All') return list;
    return list.filter((p) => p.status === filter);
  });

  ngOnInit() {
    this.loadPayments();
    this.doctorService
      .getDoctors()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: (d) => this.doctors.set(d) });
    this.healthPlanService
      .getHealthPlans()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: (h) => this.healthPlans.set(h) });
    this.procedureService
      .getProcedures(false)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: (p) => this.procedures.set(p) });
  }

  openCreateForm() {
    this.formOpen.set(true);
  }

  closeForm() {
    this.formOpen.set(false);
  }

  openDetail(payment: PaymentDto) {
    this.selectedPayment.set(payment);
    this.detailOpen.set(true);
  }

  closeDetail() {
    this.detailOpen.set(false);
    this.selectedPayment.set(null);
  }

  onSaved(payment: PaymentDto) {
    this.payments.update((list) => [...list, payment]);
    this.closeForm();
  }

  onDetailUpdated(payment: PaymentDto) {
    this.payments.update((list) => list.map((p) => (p.id === payment.id ? payment : p)));
    this.selectedPayment.set(payment);
  }

  setStatusFilter(status: PaymentStatus | 'All') {
    this.statusFilter.set(status);
  }

  doctorName(doctorId: string): string {
    return this.doctors().find((d) => d.id === doctorId)?.name ?? '—';
  }

  healthPlanName(healthPlanId: string): string {
    return this.healthPlans().find((h) => h.id === healthPlanId)?.name ?? '—';
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

  totalValue(payment: PaymentDto): number {
    return payment.items.reduce((sum, item) => sum + item.value, 0);
  }

  formatDate(dateStr: string): string {
    const [year, month, day] = dateStr.split('-');
    return `${day}/${month}/${year}`;
  }

  private loadPayments() {
    this.paymentService
      .getPayments()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (payments) => this.payments.set(payments),
        error: () => this.errorMessage.set('Erro ao carregar pagamentos.'),
      });
  }
}
