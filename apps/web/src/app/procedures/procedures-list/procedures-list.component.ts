import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  signal,
} from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ProcedureService, ProcedureDto, ProcedureImportDto } from '../data-access/procedure.service';
import { ProcedureFormComponent } from '../procedure-form/procedure-form.component';
import { ProcedureImportComponent } from '../procedure-import/procedure-import.component';

@Component({
  selector: 'app-procedures-list',
  standalone: true,
  imports: [ProcedureFormComponent, ProcedureImportComponent, CurrencyPipe, DatePipe],
  templateUrl: './procedures-list.component.html',
  styleUrl: './procedures-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProceduresListComponent implements OnInit {
  private readonly procedureService = inject(ProcedureService);
  private readonly destroyRef = inject(DestroyRef);

  readonly procedures = signal<ProcedureDto[]>([]);
  readonly formOpen = signal(false);
  readonly importOpen = signal(false);
  readonly selectedProcedure = signal<ProcedureDto | null>(null);
  readonly errorMessage = signal('');
  readonly showAllVigencias = signal(false);

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

  openImport() {
    this.importOpen.set(true);
  }

  closeImport() {
    this.importOpen.set(false);
  }

  onImported(_dto: ProcedureImportDto) {
    this.importOpen.set(false);
    this.loadProcedures();
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

  toggleVigencias() {
    this.showAllVigencias.update((v) => !v);
    this.loadProcedures();
  }

  vigenciaLabel(procedure: ProcedureDto): string {
    const from = procedure.effectiveFrom;
    const to = procedure.effectiveTo ?? 'em vigor';
    return `${from} – ${to}`;
  }

  private loadProcedures() {
    const activeOnly = !this.showAllVigencias();
    this.procedureService
      .getProcedures(activeOnly)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (procedures) => this.procedures.set(procedures),
        error: () => this.errorMessage.set('Erro ao carregar procedimentos.'),
      });
  }
}
