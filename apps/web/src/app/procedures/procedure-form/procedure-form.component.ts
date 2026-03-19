import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { HttpErrorResponse } from '@angular/common/http';
import { ProcedureService, ProcedureDto } from '../data-access/procedure.service';

@Component({
  selector: 'app-procedure-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './procedure-form.component.html',
  styleUrl: './procedure-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProcedureFormComponent implements OnChanges {
  @Input() procedure: ProcedureDto | null = null;
  @Output() readonly saved = new EventEmitter<ProcedureDto>();
  @Output() readonly closed = new EventEmitter<void>();

  private readonly fb = inject(FormBuilder);
  private readonly procedureService = inject(ProcedureService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(false);
  readonly errorMessage = signal('');

  readonly form = this.fb.nonNullable.group({
    code: ['', [Validators.required, Validators.maxLength(50)]],
    description: ['', [Validators.required, Validators.maxLength(512)]],
    value: [0, [Validators.required, Validators.min(0.01)]],
    effectiveFrom: ['', [Validators.required]],
    effectiveTo: [''],
  });

  get isEditing(): boolean {
    return this.procedure !== null;
  }

  ngOnChanges() {
    if (this.procedure) {
      this.form.patchValue({
        code: this.procedure.code,
        description: this.procedure.description,
        value: this.procedure.value,
        effectiveFrom: this.procedure.effectiveFrom,
        effectiveTo: this.procedure.effectiveTo ?? '',
      });
    } else {
      this.form.reset({
        code: '',
        description: '',
        value: 0,
        effectiveFrom: '',
        effectiveTo: '',
      });
    }
    this.errorMessage.set('');
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    const raw = this.form.getRawValue();

    const request$ =
      this.isEditing && this.procedure
        ? this.procedureService.updateProcedure(this.procedure.id, {
            code: raw.code,
            description: raw.description,
            value: raw.value,
            effectiveTo: raw.effectiveTo || undefined,
          })
        : this.procedureService.createProcedure({
            code: raw.code,
            description: raw.description,
            value: raw.value,
            effectiveFrom: raw.effectiveFrom,
            effectiveTo: raw.effectiveTo || undefined,
          });

    request$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (procedure) => {
        this.loading.set(false);
        this.saved.emit(procedure);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        if (err.status === 409) {
          this.errorMessage.set(
            'Já existe um procedimento com esse código nesta organização.',
          );
        } else {
          this.errorMessage.set('Erro ao salvar procedimento. Tente novamente.');
        }
      },
    });
  }

  close() {
    this.closed.emit();
  }
}
