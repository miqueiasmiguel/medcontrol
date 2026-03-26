import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  EventEmitter,
  Input,
  OnInit,
  Output,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { HttpErrorResponse } from '@angular/common/http';
import { combineLatest } from 'rxjs';
import { DoctorService, DoctorDto } from '../data-access/doctor.service';
import { MembersService, MemberDto } from '../../members/data-access/members.service';

export interface AvailableMember {
  userId: string;
  label: string;
}

@Component({
  selector: 'app-doctor-link-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './doctor-link-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DoctorLinkFormComponent implements OnInit {
  @Input() doctor!: DoctorDto;
  @Output() readonly saved = new EventEmitter<DoctorDto>();
  @Output() readonly closed = new EventEmitter<void>();

  private readonly fb = inject(FormBuilder);
  private readonly doctorService = inject(DoctorService);
  private readonly membersService = inject(MembersService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(false);
  readonly loadingMembers = signal(true);
  readonly errorMessage = signal('');
  readonly availableMembers = signal<AvailableMember[]>([]);

  readonly form = this.fb.nonNullable.group({
    userId: ['', Validators.required],
  });

  ngOnInit() {
    combineLatest([this.membersService.getMembers(), this.doctorService.getDoctors()])
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ([members, doctors]) => {
          const linkedUserIds = new Set(
            doctors.filter((d) => d.userId !== null).map((d) => d.userId as string),
          );
          const doctorMembers = members.filter(
            (m: MemberDto) => m.role === 'doctor' && !linkedUserIds.has(m.userId),
          );
          this.availableMembers.set(
            doctorMembers.map((m: MemberDto) => ({
              userId: m.userId,
              label: m.displayName ? `${m.displayName} (${m.email ?? ''})` : (m.email ?? m.userId),
            })),
          );
          this.loadingMembers.set(false);
        },
        error: () => {
          this.errorMessage.set('Erro ao carregar membros. Tente novamente.');
          this.loadingMembers.set(false);
        },
      });
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    const { userId } = this.form.getRawValue();

    this.doctorService
      .linkDoctorToUser(this.doctor.id, userId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updated) => {
          this.loading.set(false);
          this.saved.emit(updated);
        },
        error: (err: HttpErrorResponse) => {
          this.loading.set(false);
          if (err.status === 409) {
            this.errorMessage.set('Este médico já está vinculado a um usuário.');
          } else if (err.status === 400) {
            this.errorMessage.set('O usuário selecionado não é um membro médico desta organização.');
          } else {
            this.errorMessage.set('Erro ao vincular médico. Tente novamente.');
          }
        },
      });
  }

  close() {
    this.closed.emit();
  }
}
