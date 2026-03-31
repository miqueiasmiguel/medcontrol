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
import { HttpErrorResponse } from '@angular/common/http';
import { AdminTenantsService, AdminTenantDto } from '../data-access/admin-tenants.service';

@Component({
  selector: 'app-admin-tenant-form',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule],
  template: `
    <div class="atf-overlay" (click)="close()" aria-hidden="true"></div>

    <aside class="atf-panel" role="dialog" aria-modal="true" aria-label="Nova organização">
      <div class="atf-panel__header">
        <h2 class="atf-panel__title">Nova organização</h2>
        <button class="atf-panel__close" (click)="close()" type="button" aria-label="Fechar">
          <svg viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
            <path fill-rule="evenodd"
              d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414
                 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0
                 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z"
              clip-rule="evenodd"/>
          </svg>
        </button>
      </div>

      <form class="atf-panel__body" [formGroup]="form" (ngSubmit)="submit()">
        @if (errorMessage()) {
          <div class="atf-error-banner" role="alert">{{ errorMessage() }}</div>
        }

        <div class="atf-field">
          <label class="atf-label" for="name">Nome da organização</label>
          <input
            id="name"
            class="atf-input"
            [class.atf-input--error]="form.controls.name.invalid && form.controls.name.touched"
            type="text"
            formControlName="name"
            placeholder="Ex: Clínica São Paulo"
            autocomplete="off"
          />
          @if (form.controls.name.invalid && form.controls.name.touched) {
            <span class="atf-field-error">Nome é obrigatório (máximo 200 caracteres).</span>
          }
        </div>

        <div class="atf-field">
          <label class="atf-label" for="ownerEmail">E-mail do proprietário</label>
          <input
            id="ownerEmail"
            class="atf-input"
            [class.atf-input--error]="form.controls.ownerEmail.invalid && form.controls.ownerEmail.touched"
            type="email"
            formControlName="ownerEmail"
            placeholder="proprietario@exemplo.com"
            autocomplete="email"
          />
          @if (form.controls.ownerEmail.invalid && form.controls.ownerEmail.touched) {
            <span class="atf-field-error">E-mail válido é obrigatório.</span>
          }
          <span class="atf-hint">
            Se o usuário não existir, um convite será enviado por e-mail.
          </span>
        </div>

        <div class="atf-panel__footer">
          <button class="atf-btn atf-btn--ghost" type="button" (click)="close()">Cancelar</button>
          <button class="atf-btn atf-btn--primary" type="submit" [disabled]="loading()">
            @if (loading()) { Criando… } @else { Criar organização }
          </button>
        </div>
      </form>
    </aside>
  `,
  styles: [`
    :host { display: contents; }

    .atf-overlay {
      position: fixed;
      inset: 0;
      background: var(--mmc-overlay);
      z-index: 99;
      animation: fade-in 200ms ease;
    }

    .atf-panel {
      position: fixed;
      top: 0;
      right: 0;
      width: 480px;
      height: 100vh;
      background: var(--mmc-bg-card);
      box-shadow: var(--mmc-shadow-xl);
      z-index: 100;
      display: flex;
      flex-direction: column;
      animation: slide-in 200ms ease;

      &__header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: var(--mmc-space-5) var(--mmc-space-6);
        border-bottom: 1px solid var(--mmc-border);
        flex-shrink: 0;
      }

      &__title {
        font-size: var(--mmc-text-lg);
        font-weight: var(--mmc-font-weight-semibold);
        color: var(--mmc-text-primary);
        margin: 0;
      }

      &__close {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 36px;
        height: 36px;
        background: transparent;
        border: none;
        border-radius: var(--mmc-radius-md);
        color: var(--mmc-text-secondary);
        cursor: pointer;
        transition: background 150ms ease, color 150ms ease;

        svg { width: 20px; height: 20px; }

        &:hover {
          background: var(--mmc-bg-card-hover);
          color: var(--mmc-text-primary);
        }
      }

      &__body {
        flex: 1;
        overflow-y: auto;
        padding: var(--mmc-space-6);
        display: flex;
        flex-direction: column;
        gap: var(--mmc-space-5);
      }

      &__footer {
        display: flex;
        align-items: center;
        justify-content: flex-end;
        gap: var(--mmc-space-3);
        padding-top: var(--mmc-space-5);
        margin-top: auto;
        border-top: 1px solid var(--mmc-border);
      }
    }

    .atf-error-banner {
      background: var(--mmc-error-light);
      color: var(--mmc-error-dark);
      border: 1px solid var(--mmc-error);
      border-radius: var(--mmc-radius-md);
      padding: var(--mmc-space-3) var(--mmc-space-4);
      font-size: var(--mmc-text-sm);
    }

    .atf-field {
      display: flex;
      flex-direction: column;
      gap: var(--mmc-space-1-5);
    }

    .atf-label {
      font-size: var(--mmc-text-sm);
      font-weight: var(--mmc-font-weight-medium);
      color: var(--mmc-text-primary);
    }

    .atf-input {
      width: 100%;
      padding: var(--mmc-space-2-5) var(--mmc-space-3);
      font-size: var(--mmc-text-sm);
      font-family: var(--mmc-font-sans);
      color: var(--mmc-text-primary);
      background: var(--mmc-bg-card);
      border: 1px solid var(--mmc-border);
      border-radius: var(--mmc-radius-md);
      outline: none;
      box-sizing: border-box;
      transition: border-color 150ms ease, box-shadow 150ms ease;

      &::placeholder { color: var(--mmc-text-tertiary); }

      &:hover { border-color: var(--mmc-border-strong); }

      &:focus {
        border-color: var(--mmc-action-primary);
        box-shadow: var(--mmc-shadow-focus);
      }

      &--error {
        border-color: var(--mmc-error);
        &:focus { box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.25); }
      }
    }

    .atf-field-error {
      font-size: var(--mmc-text-xs);
      color: var(--mmc-error);
    }

    .atf-hint {
      font-size: var(--mmc-text-xs);
      color: var(--mmc-text-secondary);
    }

    .atf-btn {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      gap: var(--mmc-space-2);
      padding: var(--mmc-space-2-5) var(--mmc-space-4);
      font-size: var(--mmc-text-sm);
      font-weight: var(--mmc-font-weight-medium);
      border-radius: var(--mmc-radius-md);
      border: none;
      cursor: pointer;
      transition: background 150ms ease, box-shadow 150ms ease;
      white-space: nowrap;

      &--primary {
        background: var(--mmc-action-primary);
        color: var(--mmc-action-primary-text);

        &:hover:not(:disabled) {
          background: var(--mmc-action-primary-hover);
          box-shadow: var(--mmc-shadow-brand);
        }

        &:disabled { opacity: 0.6; cursor: not-allowed; }
      }

      &--ghost {
        background: transparent;
        color: var(--mmc-text-secondary);
        border: 1px solid var(--mmc-border);

        &:hover {
          background: var(--mmc-bg-card-hover);
          color: var(--mmc-text-primary);
        }
      }
    }

    @keyframes fade-in {
      from { opacity: 0; }
      to   { opacity: 1; }
    }

    @keyframes slide-in {
      from { transform: translateX(100%); }
      to   { transform: translateX(0); }
    }
  `],
})
export class AdminTenantFormComponent {
  @Output() readonly saved = new EventEmitter<AdminTenantDto>();
  @Output() readonly closed = new EventEmitter<void>();

  private readonly fb = inject(FormBuilder);
  private readonly adminService = inject(AdminTenantsService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(false);
  readonly errorMessage = signal('');

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    ownerEmail: ['', [Validators.required, Validators.email]],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    const { name, ownerEmail } = this.form.getRawValue();

    this.adminService
      .createTenant(name, ownerEmail)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (tenant) => {
          this.loading.set(false);
          this.saved.emit(tenant);
        },
        error: (err: HttpErrorResponse) => {
          this.loading.set(false);
          if (err.status === 409) {
            this.errorMessage.set('Já existe uma organização com esse nome.');
          } else {
            this.errorMessage.set('Erro ao criar organização. Tente novamente.');
          }
        },
      });
  }

  close(): void {
    this.closed.emit();
  }
}
