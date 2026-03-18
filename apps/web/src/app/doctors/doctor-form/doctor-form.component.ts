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
import { DoctorService, DoctorDto } from '../data-access/doctor.service';

@Component({
  selector: 'app-doctor-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './doctor-form.component.html',
  styleUrl: './doctor-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DoctorFormComponent implements OnChanges {
  @Input() doctor: DoctorDto | null = null;
  @Output() readonly saved = new EventEmitter<DoctorDto>();
  @Output() readonly closed = new EventEmitter<void>();

  private readonly fb = inject(FormBuilder);
  private readonly doctorService = inject(DoctorService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(false);
  readonly errorMessage = signal('');

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    crm: ['', [Validators.required, Validators.pattern(/^\d+$/)]],
    councilState: ['', [Validators.required, Validators.pattern(/^[A-Z]{2}$/)]],
    specialty: ['', [Validators.required]],
  });

  get isEditing(): boolean {
    return this.doctor !== null;
  }

  ngOnChanges() {
    if (this.doctor) {
      this.form.patchValue({
        name: this.doctor.name,
        crm: this.doctor.crm,
        councilState: this.doctor.councilState,
        specialty: this.doctor.specialty,
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
      this.isEditing && this.doctor
        ? this.doctorService.updateDoctor(this.doctor.id, value)
        : this.doctorService.createDoctor(value);

    request$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (doctor) => {
        this.loading.set(false);
        this.saved.emit(doctor);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        if (err.status === 409) {
          this.errorMessage.set('Já existe um médico com esse CRM nesta organização.');
        } else {
          this.errorMessage.set('Erro ao salvar médico. Tente novamente.');
        }
      },
    });
  }

  close() {
    this.closed.emit();
  }
}
