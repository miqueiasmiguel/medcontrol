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
import { timer } from 'rxjs';
import { MembersService, MemberDto } from '../data-access/members.service';

export const ROLE_OPTIONS = [
  { value: 'admin', label: 'Admin' },
  { value: 'operator', label: 'Operador' },
  { value: 'doctor', label: 'Médico' },
];

@Component({
  selector: 'app-member-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './member-form.component.html',
  styleUrl: './member-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MemberFormComponent implements OnChanges {
  @Input() member: MemberDto | null = null;
  @Output() readonly saved = new EventEmitter<MemberDto>();
  @Output() readonly closed = new EventEmitter<void>();

  private readonly fb = inject(FormBuilder);
  private readonly membersService = inject(MembersService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(false);
  readonly errorMessage = signal('');
  readonly invitedMessage = signal('');
  readonly roleOptions = ROLE_OPTIONS;

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    role: ['', [Validators.required]],
  });

  get isEditing(): boolean {
    return this.member !== null;
  }

  ngOnChanges() {
    if (this.member) {
      this.form.patchValue({ role: this.member.role });
      this.form.controls.email.disable();
    } else {
      this.form.reset();
      this.form.controls.email.enable();
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
      this.isEditing && this.member
        ? this.membersService.updateMemberRole(this.member.userId, { role: value.role })
        : this.membersService.addMember({ email: value.email, role: value.role });

    request$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (member) => {
        this.loading.set(false);
        if (member.invited) {
          this.invitedMessage.set('Convite enviado! O usuário receberá um e-mail para acessar o MedControl.');
          timer(2500)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe(() => {
              this.invitedMessage.set('');
              this.saved.emit(member);
            });
        } else {
          this.saved.emit(member);
        }
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        if (err.status === 409) {
          this.errorMessage.set('Este usuário já é membro desta organização.');
        } else {
          this.errorMessage.set('Erro ao salvar membro. Tente novamente.');
        }
      },
    });
  }

  close() {
    this.closed.emit();
  }
}
