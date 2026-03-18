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
import { HealthPlanService, HealthPlanDto } from '../data-access/health-plan.service';

@Component({
  selector: 'app-health-plan-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './health-plan-form.component.html',
  styleUrl: './health-plan-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HealthPlanFormComponent implements OnChanges {
  @Input() healthPlan: HealthPlanDto | null = null;
  @Output() readonly saved = new EventEmitter<HealthPlanDto>();
  @Output() readonly closed = new EventEmitter<void>();

  private readonly fb = inject(FormBuilder);
  private readonly healthPlanService = inject(HealthPlanService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(false);
  readonly errorMessage = signal('');

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(256)]],
    tissCode: ['', [Validators.required, Validators.maxLength(20)]],
  });

  get isEditing(): boolean {
    return this.healthPlan !== null;
  }

  ngOnChanges() {
    if (this.healthPlan) {
      this.form.patchValue({
        name: this.healthPlan.name,
        tissCode: this.healthPlan.tissCode,
      });
    } else {
      this.form.reset();
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

    const value = this.form.getRawValue();

    const request$ =
      this.isEditing && this.healthPlan
        ? this.healthPlanService.updateHealthPlan(this.healthPlan.id, value)
        : this.healthPlanService.createHealthPlan(value);

    request$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (healthPlan) => {
        this.loading.set(false);
        this.saved.emit(healthPlan);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        if (err.status === 409) {
          this.errorMessage.set('Já existe um convênio com esse código TISS nesta organização.');
        } else {
          this.errorMessage.set('Erro ao salvar convênio. Tente novamente.');
        }
      },
    });
  }

  close() {
    this.closed.emit();
  }
}
