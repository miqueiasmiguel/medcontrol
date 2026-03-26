import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { HttpErrorResponse } from '@angular/common/http';
import { DoctorService } from '../data-access/doctor.service';
import { WINDOW } from '../../core/tokens/window.token';

@Component({
  selector: 'app-doctor-onboarding',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './onboarding.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DoctorOnboardingComponent {
  private readonly fb = inject(FormBuilder);
  private readonly doctorService = inject(DoctorService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly win = inject(WINDOW);

  readonly loading = signal(false);
  readonly errorMessage = signal('');

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    crm: ['', [Validators.required, Validators.pattern(/^\d+$/)]],
    councilState: ['', [Validators.required, Validators.pattern(/^[A-Z]{2}$/)]],
    specialty: ['', [Validators.required]],
  });

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    this.doctorService
      .createMyDoctorProfile(this.form.getRawValue())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.loading.set(false);
          this.router.navigate(['/doctors']);
        },
        error: (err: HttpErrorResponse) => {
          this.loading.set(false);
          if (err.status === 409) {
            this.errorMessage.set('Já existe um perfil de médico vinculado a sua conta nesta organização.');
          } else {
            this.errorMessage.set('Erro ao salvar perfil. Tente novamente.');
          }
        },
      });
  }

  onCouncilStateInput(event: Event) {
    const input = event.target as HTMLInputElement;
    this.form.controls.councilState.setValue(input.value.toUpperCase(), { emitEvent: false });
  }

  skip() {
    this.win.sessionStorage.setItem('mmc_onboarding_skip', '1');
    this.router.navigate(['/doctors']);
  }
}
