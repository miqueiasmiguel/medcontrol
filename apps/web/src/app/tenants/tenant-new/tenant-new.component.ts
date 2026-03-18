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
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TenantService } from '../data-access/tenant.service';

@Component({
  selector: 'app-tenant-new',
  standalone: true,
  imports: [ReactiveFormsModule, MatProgressSpinnerModule],
  styles: [
    `
      :host {
        display: flex;
        min-height: 100vh;
      }

      .page {
        display: flex;
        width: 100%;
        min-height: 100vh;
        font-family: var(--mmc-font-sans);
      }

      /* ─── Painel de boas-vindas ─────────────────────────────── */

      .welcome-panel {
        flex: 1 1 50%;
        background: var(--mmc-navy-950);
        padding: var(--mmc-space-16) 56px;
        display: flex;
        flex-direction: column;
        justify-content: center;
        position: relative;
        overflow: hidden;

        &::before {
          content: '';
          position: absolute;
          top: -120px;
          right: -120px;
          width: 420px;
          height: 420px;
          border-radius: 50%;
          background: radial-gradient(
            circle,
            rgba(249, 115, 22, 0.07) 0%,
            transparent 70%
          );
          pointer-events: none;
        }

        &::after {
          content: '';
          position: absolute;
          bottom: -80px;
          left: -60px;
          width: 300px;
          height: 300px;
          border-radius: 50%;
          background: radial-gradient(
            circle,
            rgba(249, 115, 22, 0.04) 0%,
            transparent 70%
          );
          pointer-events: none;
        }
      }

      .welcome-brand {
        display: flex;
        align-items: center;
        gap: var(--mmc-space-3);
        margin-bottom: var(--mmc-space-16);
      }

      .welcome-brand-icon {
        width: 32px;
        height: 32px;
        background: var(--mmc-orange-500);
        border-radius: var(--mmc-radius-md);
        display: flex;
        align-items: center;
        justify-content: center;
        color: #fff;
        font-size: 18px;
        font-weight: 700;
        line-height: 1;
      }

      .welcome-brand-name {
        font-size: var(--mmc-text-sm);
        font-weight: var(--mmc-font-weight-semibold);
        color: var(--mmc-text-on-dark);
        letter-spacing: 0.04em;
        text-transform: uppercase;
      }

      .welcome-eyebrow {
        font-size: var(--mmc-text-xs);
        font-weight: var(--mmc-font-weight-medium);
        letter-spacing: 0.14em;
        text-transform: uppercase;
        color: var(--mmc-orange-400);
        margin-bottom: var(--mmc-space-4);
      }

      .welcome-title {
        font-size: var(--mmc-text-4xl);
        font-weight: var(--mmc-font-weight-bold);
        line-height: 1.15;
        color: var(--mmc-text-on-dark);
        margin: 0 0 var(--mmc-space-5);
      }

      .welcome-title-accent {
        color: var(--mmc-orange-400);
      }

      .welcome-description {
        font-size: var(--mmc-text-sm);
        font-weight: var(--mmc-font-weight-regular);
        line-height: 1.75;
        color: var(--mmc-text-on-dark-subtle);
        max-width: 380px;
        margin-bottom: var(--mmc-space-12);
      }

      .benefits-list {
        display: flex;
        flex-direction: column;
        gap: var(--mmc-space-5);
      }

      .benefit {
        display: flex;
        align-items: flex-start;
        gap: var(--mmc-space-4);
      }

      .benefit-dot {
        width: 6px;
        height: 6px;
        background: var(--mmc-orange-500);
        border-radius: 50%;
        margin-top: 6px;
        flex-shrink: 0;
      }

      .benefit-content {
        strong {
          display: block;
          font-size: var(--mmc-text-sm);
          font-weight: var(--mmc-font-weight-medium);
          color: var(--mmc-text-on-dark);
          margin-bottom: 2px;
        }

        span {
          font-size: var(--mmc-text-xs);
          color: var(--mmc-text-on-dark-subtle);
          line-height: 1.55;
        }
      }

      /* ─── Painel do formulário ──────────────────────────────── */

      .form-panel {
        flex: 1 1 50%;
        background: var(--mmc-bg);
        display: flex;
        align-items: center;
        justify-content: center;
        padding: var(--mmc-space-16) var(--mmc-space-14);
      }

      .form-card {
        background: var(--mmc-bg-card);
        border: 1px solid var(--mmc-border);
        border-radius: var(--mmc-radius-lg);
        box-shadow: var(--mmc-shadow-md);
        padding: var(--mmc-space-10) var(--mmc-space-8);
        width: 100%;
        max-width: 400px;
      }

      .form-eyebrow {
        font-size: var(--mmc-text-xs);
        font-weight: var(--mmc-font-weight-medium);
        letter-spacing: 0.1em;
        text-transform: uppercase;
        color: var(--mmc-orange-500);
        margin-bottom: var(--mmc-space-2);
      }

      .form-title {
        font-size: var(--mmc-text-2xl);
        font-weight: var(--mmc-font-weight-semibold);
        color: var(--mmc-text-primary);
        margin: 0 0 var(--mmc-space-2);
      }

      .form-subtitle {
        font-size: var(--mmc-text-sm);
        color: var(--mmc-text-secondary);
        line-height: 1.65;
        margin-bottom: var(--mmc-space-8);
      }

      .form-field {
        display: flex;
        flex-direction: column;
        gap: var(--mmc-space-1-5);
        margin-bottom: var(--mmc-space-6);

        label {
          font-size: var(--mmc-text-sm);
          font-weight: var(--mmc-font-weight-medium);
          color: var(--mmc-text-primary);
        }

        input {
          width: 100%;
          height: 40px;
          padding: 0 var(--mmc-space-3);
          font-size: var(--mmc-text-sm);
          font-family: var(--mmc-font-sans);
          color: var(--mmc-text-primary);
          background: var(--mmc-bg-card);
          border: 1px solid var(--mmc-border);
          border-radius: var(--mmc-radius-md);
          outline: none;
          box-sizing: border-box;
          transition:
            border-color 150ms ease,
            box-shadow 150ms ease;

          &::placeholder {
            color: var(--mmc-text-tertiary);
          }

          &:hover {
            border-color: var(--mmc-border-strong);
          }

          &:focus {
            border-color: var(--mmc-orange-400);
            box-shadow: var(--mmc-shadow-focus);
          }

          &.error {
            border-color: var(--mmc-error);

            &:focus {
              box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.2);
            }
          }
        }
      }

      .field-hint {
        font-size: var(--mmc-text-xs);
        color: var(--mmc-text-tertiary);
        line-height: 1.5;
      }

      .field-error {
        font-size: var(--mmc-text-xs);
        color: var(--mmc-error);
      }

      .form-error {
        font-size: var(--mmc-text-sm);
        color: var(--mmc-error);
        background: var(--mmc-error-light);
        border: 1px solid rgba(239, 68, 68, 0.2);
        border-radius: var(--mmc-radius-md);
        padding: var(--mmc-space-3) var(--mmc-space-4);
        margin-bottom: var(--mmc-space-5);
        line-height: 1.5;
      }

      .btn-primary {
        display: flex;
        align-items: center;
        justify-content: center;
        gap: var(--mmc-space-2);
        width: 100%;
        height: 40px;
        font-size: var(--mmc-text-sm);
        font-weight: var(--mmc-font-weight-semibold);
        font-family: var(--mmc-font-sans);
        color: var(--mmc-action-primary-text);
        background: var(--mmc-action-primary);
        border: none;
        border-radius: var(--mmc-radius-md);
        cursor: pointer;
        transition:
          background 150ms ease,
          box-shadow 150ms ease,
          transform 100ms ease;

        &:hover:not(:disabled) {
          background: var(--mmc-action-primary-hover);
          box-shadow: var(--mmc-shadow-brand);
        }

        &:active:not(:disabled) {
          background: var(--mmc-action-primary-active);
          transform: translateY(1px);
        }

        &:disabled {
          opacity: 0.45;
          cursor: not-allowed;
        }
      }

      .footer-note {
        margin-top: var(--mmc-space-5);
        font-size: var(--mmc-text-xs);
        color: var(--mmc-text-tertiary);
        text-align: center;
        line-height: 1.65;
      }

      /* ─── Responsivo ────────────────────────────────────────── */

      @media (max-width: 768px) {
        .page {
          flex-direction: column;
        }

        .welcome-panel {
          flex: none;
          padding: var(--mmc-space-12) var(--mmc-space-8);
        }

        .welcome-brand {
          margin-bottom: var(--mmc-space-10);
        }

        .welcome-title {
          font-size: var(--mmc-text-3xl);
        }

        .welcome-description {
          margin-bottom: var(--mmc-space-6);
        }

        .benefits-list {
          display: none;
        }

        .form-panel {
          flex: none;
          padding: var(--mmc-space-10) var(--mmc-space-6);
        }
      }
    `,
  ],
  template: `
    <div class="page">

      <!-- Painel de boas-vindas -->
      <div class="welcome-panel">
        <div class="welcome-brand">
          <div class="welcome-brand-icon">+</div>
          <span class="welcome-brand-name">MedControl</span>
        </div>

        <p class="welcome-eyebrow">Bem-vindo</p>

        <h1 class="welcome-title">
          Sua prática,<br>
          <span class="welcome-title-accent">organizada</span><br>
          do jeito certo.
        </h1>

        <p class="welcome-description">
          MedControl foi criado para médicos, clínicas e equipes de faturamento
          que precisam de clareza total sobre seus pagamentos. Você está a um
          passo de ter tudo em um só lugar.
        </p>

        <div class="benefits-list">
          <div class="benefit">
            <div class="benefit-dot"></div>
            <div class="benefit-content">
              <strong>Controle total de pagamentos</strong>
              <span>Registre e acompanhe cada procedimento em tempo real</span>
            </div>
          </div>
          <div class="benefit">
            <div class="benefit-dot"></div>
            <div class="benefit-content">
              <strong>Equipe com papéis distintos</strong>
              <span>Operadores, médicos e admins — cada um vê o que precisa</span>
            </div>
          </div>
          <div class="benefit">
            <div class="benefit-dot"></div>
            <div class="benefit-content">
              <strong>Visibilidade pelo celular</strong>
              <span>Médicos acompanham seus pagamentos direto no app mobile</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Painel do formulário -->
      <div class="form-panel">
        <div class="form-card">
          <p class="form-eyebrow">Configuração inicial</p>
          <h2 class="form-title">Crie sua organização</h2>
          <p class="form-subtitle">
            Sua organização é o espaço central onde sua equipe vai trabalhar.
            Use o nome da sua clínica, consultório ou empresa de faturamento.
          </p>

          <form [formGroup]="form" (ngSubmit)="onSubmit()" novalidate>
            <div class="form-field">
              <label for="org-name">Nome da organização</label>
              <input
                id="org-name"
                type="text"
                formControlName="name"
                placeholder="Ex: Clínica São Lucas"
                autocomplete="organization"
                [class.error]="form.get('name')?.invalid && form.get('name')?.touched"
              />
              @if (form.get('name')?.invalid && form.get('name')?.touched) {
                <span class="field-error">Informe o nome da organização para continuar.</span>
              } @else {
                <span class="field-hint">Você pode alterar este nome nas configurações a qualquer momento.</span>
              }
            </div>

            @if (errorMessage()) {
              <div class="form-error">{{ errorMessage() }}</div>
            }

            <button type="submit" class="btn-primary" [disabled]="loading()">
              @if (loading()) {
                <mat-spinner diameter="16" />
              }
              Criar organização e começar
            </button>

            <p class="footer-note">
              Ao criar sua organização, você concorda com os<br>Termos de Uso e Política de Privacidade do MedControl.
            </p>
          </form>
        </div>
      </div>

    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TenantNewComponent {
  private readonly fb = inject(FormBuilder);
  private readonly tenantService = inject(TenantService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(false);
  readonly errorMessage = signal('');

  readonly form = this.fb.group({
    name: ['', [Validators.required]],
  });

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    this.tenantService
      .createTenant(this.form.value.name!)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.loading.set(false);
          this.router.navigate(['/']);
        },
        error: () => {
          this.loading.set(false);
          this.errorMessage.set('Erro ao criar organização. Tente novamente.');
        },
      });
  }
}
