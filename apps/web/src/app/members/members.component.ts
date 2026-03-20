import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  signal,
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MembersService, MemberDto } from './data-access/members.service';
import { MemberFormComponent } from './member-form/member-form.component';

export const ROLE_LABELS: Record<string, string> = {
  admin: 'Admin',
  operator: 'Operador',
  doctor: 'Médico',
  owner: 'Proprietário',
};

@Component({
  selector: 'app-members',
  standalone: true,
  imports: [MemberFormComponent, DatePipe],
  templateUrl: './members.component.html',
  styleUrl: './members.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MembersComponent implements OnInit {
  private readonly membersService = inject(MembersService);
  private readonly destroyRef = inject(DestroyRef);

  readonly members = signal<MemberDto[]>([]);
  readonly loading = signal(false);
  readonly formOpen = signal(false);
  readonly selectedMember = signal<MemberDto | null>(null);
  readonly errorMessage = signal('');
  readonly roleLabels = ROLE_LABELS;

  ngOnInit() {
    this.loadMembers();
  }

  openAddForm() {
    this.selectedMember.set(null);
    this.formOpen.set(true);
  }

  openEditForm(member: MemberDto) {
    this.selectedMember.set(member);
    this.formOpen.set(true);
  }

  closeForm() {
    this.formOpen.set(false);
    this.selectedMember.set(null);
  }

  onSaved(member: MemberDto) {
    this.members.update((list) => {
      const index = list.findIndex((m) => m.userId === member.userId);
      if (index >= 0) {
        return list.map((m) => (m.userId === member.userId ? member : m));
      }
      return [...list, member];
    });
    this.closeForm();
  }

  removeMember(member: MemberDto) {
    this.membersService
      .removeMember(member.userId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.members.update((list) => list.filter((m) => m.userId !== member.userId));
        },
        error: () => {
          this.errorMessage.set('Erro ao remover membro. Tente novamente.');
        },
      });
  }

  getRoleLabel(role: string): string {
    return this.roleLabels[role] ?? role;
  }

  private loadMembers() {
    this.loading.set(true);
    this.membersService
      .getMembers()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (members) => {
          this.members.set(members);
          this.loading.set(false);
        },
        error: () => {
          this.errorMessage.set('Erro ao carregar membros.');
          this.loading.set(false);
        },
      });
  }
}
