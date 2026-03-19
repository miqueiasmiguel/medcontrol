import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  EventEmitter,
  OnInit,
  Output,
  inject,
  signal,
} from '@angular/core';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { HttpErrorResponse } from '@angular/common/http';
import { PaymentService, PaymentDto } from '../data-access/payment.service';
import { DoctorService, DoctorDto } from '../../doctors/data-access/doctor.service';
import { HealthPlanService, HealthPlanDto } from '../../health-plans/data-access/health-plan.service';
import { ProcedureService, ProcedureDto } from '../../procedures/data-access/procedure.service';

@Component({
  selector: 'app-payment-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './payment-form.component.html',
  styleUrl: './payment-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaymentFormComponent implements OnInit {
  @Output() readonly saved = new EventEmitter<PaymentDto>();
  @Output() readonly closed = new EventEmitter<void>();

  private readonly fb = inject(FormBuilder);
  private readonly paymentService = inject(PaymentService);
  private readonly doctorService = inject(DoctorService);
  private readonly healthPlanService = inject(HealthPlanService);
  private readonly procedureService = inject(ProcedureService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(false);
  readonly errorMessage = signal('');
  readonly doctors = signal<DoctorDto[]>([]);
  readonly healthPlans = signal<HealthPlanDto[]>([]);
  readonly procedures = signal<ProcedureDto[]>([]);

  readonly form = this.fb.group({
    doctorId: ['', Validators.required],
    healthPlanId: ['', Validators.required],
    executionDate: ['', Validators.required],
    appointmentNumber: ['', [Validators.required, Validators.maxLength(100)]],
    authorizationCode: [''],
    beneficiaryCard: ['', [Validators.required, Validators.maxLength(50)]],
    beneficiaryName: ['', [Validators.required, Validators.maxLength(256)]],
    executionLocation: ['', [Validators.required, Validators.maxLength(256)]],
    paymentLocation: ['', [Validators.required, Validators.maxLength(256)]],
    notes: [''],
    items: this.fb.array([this.buildItem()]),
  });

  get itemsArray(): FormArray<FormGroup> {
    return this.form.get('items') as FormArray<FormGroup>;
  }

  ngOnInit() {
    this.doctorService
      .getDoctors()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: (d) => this.doctors.set(d) });
    this.healthPlanService
      .getHealthPlans()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: (h) => this.healthPlans.set(h) });
    this.procedureService
      .getProcedures()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: (p) => this.procedures.set(p) });
  }

  buildItem(): FormGroup {
    return this.fb.group({
      procedureId: ['', Validators.required],
      value: [0, [Validators.required, Validators.min(0.01)]],
    });
  }

  addItem() {
    this.itemsArray.push(this.buildItem());
  }

  removeItem(index: number) {
    if (this.itemsArray.length > 1) {
      this.itemsArray.removeAt(index);
    }
  }

  onProcedureSelected(index: number) {
    const item = this.itemsArray.at(index);
    const procedureId = item.get('procedureId')?.value as string;
    const procedure = this.procedures().find((p) => p.id === procedureId);
    if (procedure) {
      item.patchValue({ value: procedure.value });
    }
  }

  procedureLabel(procedure: ProcedureDto): string {
    return `${procedure.code} — ${procedure.description}`;
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    const raw = this.form.getRawValue();
    const items = this.itemsArray.getRawValue() as Array<{ procedureId: string; value: number }>;

    this.paymentService
      .createPayment({
        doctorId: raw.doctorId ?? '',
        healthPlanId: raw.healthPlanId ?? '',
        executionDate: raw.executionDate ?? '',
        appointmentNumber: raw.appointmentNumber ?? '',
        authorizationCode: raw.authorizationCode || undefined,
        beneficiaryCard: raw.beneficiaryCard ?? '',
        beneficiaryName: raw.beneficiaryName ?? '',
        executionLocation: raw.executionLocation ?? '',
        paymentLocation: raw.paymentLocation ?? '',
        notes: raw.notes || undefined,
        items: items.map((i) => ({ procedureId: i.procedureId, value: i.value })),
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (payment) => {
          this.loading.set(false);
          this.saved.emit(payment);
        },
        error: (err: HttpErrorResponse) => {
          this.loading.set(false);
          if (err.status === 400) {
            this.errorMessage.set('Dados inválidos. Verifique os campos e tente novamente.');
          } else {
            this.errorMessage.set('Erro ao registrar pagamento. Tente novamente.');
          }
        },
      });
  }

  close() {
    this.closed.emit();
  }
}
