import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  signal,
} from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ProcedureService, ProcedureDto } from '../data-access/procedure.service';
import { ProcedureFormComponent } from '../procedure-form/procedure-form.component';

@Component({
  selector: 'app-procedures-list',
  standalone: true,
  imports: [ProcedureFormComponent, CurrencyPipe],
  templateUrl: './procedures-list.component.html',
  styleUrl: './procedures-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProceduresListComponent implements OnInit {
  private readonly procedureService = inject(ProcedureService);
  private readonly destroyRef = inject(DestroyRef);

  readonly procedures = signal<ProcedureDto[]>([]);
  readonly formOpen = signal(false);
  readonly selectedProcedure = signal<ProcedureDto | null>(null);
  readonly errorMessage = signal('');

  ngOnInit() {
    this.loadProcedures();
  }

  openCreateForm() {
    this.selectedProcedure.set(null);
    this.formOpen.set(true);
  }

  openEditForm(procedure: ProcedureDto) {
    this.selectedProcedure.set(procedure);
    this.formOpen.set(true);
  }

  closeForm() {
    this.formOpen.set(false);
    this.selectedProcedure.set(null);
  }

  onSaved(procedure: ProcedureDto) {
    this.procedures.update((list) => {
      const index = list.findIndex((p) => p.id === procedure.id);
      if (index >= 0) {
        return list.map((p) => (p.id === procedure.id ? procedure : p));
      }
      return [...list, procedure];
    });
    this.closeForm();
  }

  private loadProcedures() {
    this.procedureService
      .getProcedures()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (procedures) => this.procedures.set(procedures),
        error: () => this.errorMessage.set('Erro ao carregar procedimentos.'),
      });
  }
}
