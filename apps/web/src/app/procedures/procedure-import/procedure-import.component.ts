import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  EventEmitter,
  Output,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ProcedureService, ProcedureImportDto } from '../data-access/procedure.service';

@Component({
  selector: 'app-procedure-import',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './procedure-import.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProcedureImportComponent {
  @Output() readonly imported = new EventEmitter<ProcedureImportDto>();
  @Output() readonly closed = new EventEmitter<void>();

  private readonly fb = inject(FormBuilder);
  private readonly procedureService = inject(ProcedureService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(false);
  readonly errorMessage = signal('');
  readonly result = signal<ProcedureImportDto | null>(null);

  selectedFile: File | null = null;

  readonly form = this.fb.nonNullable.group({
    source: ['', [Validators.required]],
    effectiveFrom: ['', [Validators.required]],
  });

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    this.selectedFile = input.files?.[0] ?? null;
  }

  submit() {
    if (this.form.invalid || !this.selectedFile) {
      this.form.markAllAsTouched();
      if (!this.selectedFile) {
        this.errorMessage.set('Selecione um arquivo CSV.');
      }
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');
    this.result.set(null);

    const { source, effectiveFrom } = this.form.getRawValue();

    this.procedureService
      .importProcedures(this.selectedFile, source, effectiveFrom)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (dto) => {
          this.loading.set(false);
          this.result.set(dto);
          this.imported.emit(dto);
        },
        error: () => {
          this.loading.set(false);
          this.errorMessage.set('Erro ao importar procedimentos. Verifique o arquivo e tente novamente.');
        },
      });
  }

  close() {
    this.closed.emit();
  }
}
