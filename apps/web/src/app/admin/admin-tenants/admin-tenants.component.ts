import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  inject,
  signal,
  computed,
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AdminTenantsService, AdminTenantDto } from '../data-access/admin-tenants.service';
import { AdminTenantFormComponent } from '../admin-tenant-form/admin-tenant-form.component';

@Component({
  selector: 'app-admin-tenants',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, MatProgressSpinnerModule, AdminTenantFormComponent],
  template: `
    <div class="at-page">

      <!-- Page header -->
      <div class="at-page-header">
        <div class="at-page-header__title-group">
          <h1 class="at-title">Organizações</h1>
          <p class="at-subtitle">Gerencie e controle todas as organizações da plataforma</p>
        </div>
        <button class="at-new-btn" type="button" (click)="openForm()">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor"
               stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
            <line x1="12" y1="5" x2="12" y2="19"/>
            <line x1="5" y1="12" x2="19" y2="12"/>
          </svg>
          Nova organização
        </button>
      </div>

      @if (loading()) {
        <div class="at-loading">
          <mat-spinner diameter="36" />
          <span class="at-loading__label">Carregando organizações…</span>
        </div>
      } @else {

        <!-- Stats row -->
        <div class="at-stats">
          <div class="at-stat">
            <div class="at-stat__icon at-stat__icon--brand">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                <rect x="2" y="7" width="20" height="14" rx="2"/>
                <path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/>
              </svg>
            </div>
            <div class="at-stat__body">
              <span class="at-stat__value">{{ tenants().length }}</span>
              <span class="at-stat__label">Total</span>
            </div>
          </div>

          <div class="at-stat">
            <div class="at-stat__icon at-stat__icon--success">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                <polyline points="20 6 9 17 4 12"/>
              </svg>
            </div>
            <div class="at-stat__body">
              <span class="at-stat__value">{{ activeCount() }}</span>
              <span class="at-stat__label">Ativas</span>
            </div>
          </div>

          <div class="at-stat">
            <div class="at-stat__icon at-stat__icon--muted">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                <circle cx="12" cy="12" r="10"/>
                <line x1="4.93" y1="4.93" x2="19.07" y2="19.07"/>
              </svg>
            </div>
            <div class="at-stat__body">
              <span class="at-stat__value">{{ inactiveCount() }}</span>
              <span class="at-stat__label">Inativas</span>
            </div>
          </div>

          <div class="at-stat">
            <div class="at-stat__icon at-stat__icon--info">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/>
                <circle cx="9" cy="7" r="4"/>
                <path d="M23 21v-2a4 4 0 0 0-3-3.87"/>
                <path d="M16 3.13a4 4 0 0 1 0 7.75"/>
              </svg>
            </div>
            <div class="at-stat__body">
              <span class="at-stat__value">{{ totalMembers() }}</span>
              <span class="at-stat__label">Membros</span>
            </div>
          </div>
        </div>

        <!-- Table card -->
        <div class="at-card">
          @if (tenants().length === 0) {
            <div class="at-empty">
              <div class="at-empty__icon">
                <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                  <rect x="2" y="7" width="20" height="14" rx="2"/>
                  <path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/>
                </svg>
              </div>
              <p class="at-empty__title">Nenhuma organização cadastrada</p>
              <p class="at-empty__desc">Crie a primeira organização da plataforma.</p>
              <button class="at-empty__btn" type="button" (click)="openForm()">
                Criar organização
              </button>
            </div>
          } @else {
            <table class="at-table">
              <thead class="at-thead">
                <tr>
                  <th class="at-th">Organização</th>
                  <th class="at-th">Status</th>
                  <th class="at-th at-th--center">Membros</th>
                  <th class="at-th">Criado em</th>
                  <th class="at-th at-th--end">Ações</th>
                </tr>
              </thead>
              <tbody>
                @for (tenant of tenants(); track tenant.id) {
                  <tr class="at-row" [class.at-row--inactive]="!tenant.isActive">

                    <!-- Name + slug -->
                    <td class="at-td">
                      <div class="at-org">
                        <div class="at-org__avatar" [class.at-org__avatar--inactive]="!tenant.isActive">
                          {{ getInitials(tenant.name) }}
                        </div>
                        <div class="at-org__info">
                          <span class="at-org__name">{{ tenant.name }}</span>
                          <span class="at-org__slug">{{ tenant.slug }}</span>
                        </div>
                      </div>
                    </td>

                    <!-- Status -->
                    <td class="at-td">
                      <span
                        class="at-badge"
                        [class.at-badge--active]="tenant.isActive"
                        [class.at-badge--inactive]="!tenant.isActive"
                      >
                        <span class="at-badge__dot"></span>
                        {{ tenant.isActive ? 'Ativo' : 'Inativo' }}
                      </span>
                    </td>

                    <!-- Members -->
                    <td class="at-td at-td--center">
                      <span class="at-members">
                        <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                          <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/>
                          <circle cx="9" cy="7" r="4"/>
                          <path d="M23 21v-2a4 4 0 0 0-3-3.87"/>
                          <path d="M16 3.13a4 4 0 0 1 0 7.75"/>
                        </svg>
                        {{ tenant.memberCount }}
                      </span>
                    </td>

                    <!-- Date -->
                    <td class="at-td at-td--date">
                      {{ tenant.createdAt | date: 'dd/MM/yyyy' }}
                    </td>

                    <!-- Action -->
                    <td class="at-td at-td--end">
                      <button
                        class="at-action-btn"
                        [class.at-action-btn--deactivate]="tenant.isActive"
                        [class.at-action-btn--activate]="!tenant.isActive"
                        data-testid="toggle-status"
                        (click)="toggleStatus(tenant)"
                      >
                        @if (tenant.isActive) {
                          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                            <circle cx="12" cy="12" r="10"/>
                            <line x1="4.93" y1="4.93" x2="19.07" y2="19.07"/>
                          </svg>
                          Desativar
                        } @else {
                          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                            <polyline points="20 6 9 17 4 12"/>
                          </svg>
                          Ativar
                        }
                      </button>
                    </td>

                  </tr>
                }
              </tbody>
            </table>
          }
        </div>

      }
    </div>

    @if (formOpen()) {
      <app-admin-tenant-form
        (saved)="onSaved($event)"
        (closed)="closeForm()"
      />
    }
  `,
  styles: [`
    :host {
      display: block;
    }

    .at-page {
      padding: var(--mmc-space-8) var(--mmc-space-8);
      max-width: 1100px;
    }

    // ── Page header ─────────────────────────────────────────────────────────────

    .at-page-header {
      margin-bottom: var(--mmc-space-8);
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: var(--mmc-space-4);

      &__title-group {
        display: flex;
        flex-direction: column;
        gap: var(--mmc-space-1);
      }
    }

    .at-new-btn {
      display: inline-flex;
      align-items: center;
      gap: var(--mmc-space-2);
      height: 36px;
      padding: 0 var(--mmc-space-4);
      font-size: var(--mmc-text-sm);
      font-weight: var(--mmc-font-weight-medium);
      background: var(--mmc-action-primary);
      color: var(--mmc-action-primary-text);
      border: none;
      border-radius: var(--mmc-radius-md);
      cursor: pointer;
      white-space: nowrap;
      flex-shrink: 0;
      transition: background 150ms ease, box-shadow 150ms ease;

      &:hover {
        background: var(--mmc-action-primary-hover);
        box-shadow: var(--mmc-shadow-brand);
      }
    }

    .at-title {
      font-size: var(--mmc-text-2xl);
      font-weight: var(--mmc-font-weight-bold);
      color: var(--mmc-text-primary);
      margin: 0;
      letter-spacing: -0.3px;
    }

    .at-subtitle {
      font-size: var(--mmc-text-sm);
      color: var(--mmc-text-secondary);
      margin: 0;
    }

    // ── Loading ─────────────────────────────────────────────────────────────────

    .at-loading {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: var(--mmc-space-4);
      padding: var(--mmc-space-16) 0;

      &__label {
        font-size: var(--mmc-text-sm);
        color: var(--mmc-text-secondary);
      }
    }

    // ── Stats ───────────────────────────────────────────────────────────────────

    .at-stats {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: var(--mmc-space-4);
      margin-bottom: var(--mmc-space-6);
    }

    .at-stat {
      background: var(--mmc-bg-card);
      border: 1px solid var(--mmc-border);
      border-radius: var(--mmc-radius-lg);
      padding: var(--mmc-space-5);
      display: flex;
      align-items: center;
      gap: var(--mmc-space-4);
      box-shadow: var(--mmc-shadow-sm);

      &__icon {
        width: 42px;
        height: 42px;
        border-radius: var(--mmc-radius-md);
        display: flex;
        align-items: center;
        justify-content: center;
        flex-shrink: 0;

        &--brand {
          background: rgba(249, 115, 22, 0.1);
          color: var(--mmc-orange-500);
        }

        &--success {
          background: var(--mmc-success-light);
          color: var(--mmc-success);
        }

        &--muted {
          background: var(--mmc-bg-card-hover);
          color: var(--mmc-text-tertiary);
        }

        &--info {
          background: var(--mmc-info-light);
          color: var(--mmc-info);
        }
      }

      &__body {
        display: flex;
        flex-direction: column;
        gap: 2px;
      }

      &__value {
        font-size: var(--mmc-text-2xl);
        font-weight: var(--mmc-font-weight-bold);
        color: var(--mmc-text-primary);
        line-height: 1;
        font-variant-numeric: tabular-nums;
      }

      &__label {
        font-size: var(--mmc-text-xs);
        color: var(--mmc-text-secondary);
        font-weight: var(--mmc-font-weight-medium);
        text-transform: uppercase;
        letter-spacing: 0.4px;
      }
    }

    // ── Card ────────────────────────────────────────────────────────────────────

    .at-card {
      background: var(--mmc-bg-card);
      border: 1px solid var(--mmc-border);
      border-radius: var(--mmc-radius-lg);
      box-shadow: var(--mmc-shadow-sm);
      overflow: hidden;
    }

    // ── Empty ────────────────────────────────────────────────────────────────────

    .at-empty {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: var(--mmc-space-2);
      padding: var(--mmc-space-16) var(--mmc-space-8);
      text-align: center;

      &__icon {
        width: 64px;
        height: 64px;
        border-radius: var(--mmc-radius-xl);
        background: var(--mmc-bg-card-hover);
        display: flex;
        align-items: center;
        justify-content: center;
        color: var(--mmc-text-tertiary);
        margin-bottom: var(--mmc-space-2);
      }

      &__title {
        font-size: var(--mmc-text-md);
        font-weight: var(--mmc-font-weight-semibold);
        color: var(--mmc-text-primary);
        margin: 0;
      }

      &__desc {
        font-size: var(--mmc-text-sm);
        color: var(--mmc-text-secondary);
        margin: 0;
      }

      &__btn {
        margin-top: var(--mmc-space-2);
        display: inline-flex;
        align-items: center;
        gap: var(--mmc-space-2);
        height: 36px;
        padding: 0 var(--mmc-space-4);
        font-size: var(--mmc-text-sm);
        font-weight: var(--mmc-font-weight-medium);
        background: var(--mmc-action-primary);
        color: var(--mmc-action-primary-text);
        border: none;
        border-radius: var(--mmc-radius-md);
        cursor: pointer;
        transition: background 150ms ease, box-shadow 150ms ease;

        &:hover {
          background: var(--mmc-action-primary-hover);
          box-shadow: var(--mmc-shadow-brand);
        }
      }
    }

    // ── Table ───────────────────────────────────────────────────────────────────

    .at-table {
      width: 100%;
      border-collapse: collapse;
    }

    .at-thead {
      background: var(--mmc-bg);
      border-bottom: 1px solid var(--mmc-border);
    }

    .at-th {
      padding: var(--mmc-space-3) var(--mmc-space-5);
      font-size: var(--mmc-text-xs);
      font-weight: var(--mmc-font-weight-semibold);
      color: var(--mmc-text-secondary);
      text-align: left;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      white-space: nowrap;

      &--center { text-align: center; }
      &--end    { text-align: right; }
    }

    .at-row {
      border-bottom: 1px solid var(--mmc-divider);
      transition: background 120ms ease;

      &:last-child { border-bottom: none; }

      &:hover { background: var(--mmc-bg-card-hover); }

      &--inactive {
        .at-org__name { color: var(--mmc-text-secondary); }
        .at-org__avatar { opacity: 0.55; }
      }
    }

    .at-td {
      padding: var(--mmc-space-4) var(--mmc-space-5);
      font-size: var(--mmc-text-sm);
      color: var(--mmc-text-primary);
      vertical-align: middle;

      &--center { text-align: center; }
      &--end    { text-align: right; }

      &--date {
        font-size: var(--mmc-text-xs);
        color: var(--mmc-text-secondary);
        white-space: nowrap;
      }
    }

    // ── Org cell ────────────────────────────────────────────────────────────────

    .at-org {
      display: flex;
      align-items: center;
      gap: var(--mmc-space-3);

      &__avatar {
        width: 36px;
        height: 36px;
        border-radius: var(--mmc-radius-md);
        background: linear-gradient(135deg, var(--mmc-navy-600), var(--mmc-navy-900));
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: var(--mmc-text-xs);
        font-weight: var(--mmc-font-weight-bold);
        color: var(--mmc-neutral-0);
        letter-spacing: 0.5px;
        flex-shrink: 0;
        transition: opacity 150ms ease;
      }

      &__info {
        display: flex;
        flex-direction: column;
        gap: 2px;
        min-width: 0;
      }

      &__name {
        font-size: var(--mmc-text-sm);
        font-weight: var(--mmc-font-weight-semibold);
        color: var(--mmc-text-primary);
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
        transition: color 150ms ease;
      }

      &__slug {
        font-size: var(--mmc-text-xs);
        color: var(--mmc-text-tertiary);
        font-family: var(--mmc-font-mono);
        letter-spacing: 0.2px;
      }
    }

    // ── Badge ───────────────────────────────────────────────────────────────────

    .at-badge {
      display: inline-flex;
      align-items: center;
      gap: var(--mmc-space-1-5);
      padding: var(--mmc-space-1) var(--mmc-space-2-5);
      font-size: var(--mmc-text-xs);
      font-weight: var(--mmc-font-weight-medium);
      border-radius: var(--mmc-radius-full);
      border: 1px solid;
      white-space: nowrap;

      &__dot {
        width: 6px;
        height: 6px;
        border-radius: 50%;
        flex-shrink: 0;
      }

      &--active {
        background: var(--mmc-status-paid-bg);
        color: var(--mmc-status-paid-text);
        border-color: var(--mmc-status-paid-border);

        .at-badge__dot { background: var(--mmc-status-paid-dot); }
      }

      &--inactive {
        background: var(--mmc-bg-card-hover);
        color: var(--mmc-text-tertiary);
        border-color: var(--mmc-border);

        .at-badge__dot { background: var(--mmc-text-disabled); }
      }
    }

    // ── Members cell ────────────────────────────────────────────────────────────

    .at-members {
      display: inline-flex;
      align-items: center;
      gap: var(--mmc-space-1-5);
      font-size: var(--mmc-text-sm);
      color: var(--mmc-text-secondary);
      font-variant-numeric: tabular-nums;
    }

    // ── Action button ───────────────────────────────────────────────────────────

    .at-action-btn {
      display: inline-flex;
      align-items: center;
      gap: var(--mmc-space-1-5);
      height: 32px;
      padding: 0 var(--mmc-space-3);
      font-size: var(--mmc-text-xs);
      font-weight: var(--mmc-font-weight-semibold);
      border-radius: var(--mmc-radius-md);
      border: 1px solid;
      cursor: pointer;
      transition: background 150ms ease, color 150ms ease, border-color 150ms ease, box-shadow 150ms ease;
      white-space: nowrap;

      &--deactivate {
        background: var(--mmc-error-light);
        color: var(--mmc-error-dark);
        border-color: rgba(239, 68, 68, 0.2);

        &:hover {
          background: var(--mmc-error);
          color: var(--mmc-neutral-0);
          border-color: var(--mmc-error);
          box-shadow: 0 2px 8px rgba(239, 68, 68, 0.3);
        }
      }

      &--activate {
        background: var(--mmc-success-light);
        color: var(--mmc-success-dark);
        border-color: rgba(16, 185, 129, 0.2);

        &:hover {
          background: var(--mmc-success);
          color: var(--mmc-neutral-0);
          border-color: var(--mmc-success);
          box-shadow: 0 2px 8px rgba(16, 185, 129, 0.3);
        }
      }

      &:active {
        transform: translateY(1px);
        box-shadow: none;
      }

      &:focus-visible {
        outline: none;
        box-shadow: var(--mmc-shadow-focus);
      }
    }
  `],
})
export class AdminTenantsComponent implements OnInit {
  private readonly adminService = inject(AdminTenantsService);

  readonly tenants = signal<AdminTenantDto[]>([]);
  readonly loading = signal(true);
  readonly formOpen = signal(false);

  readonly activeCount = computed(() => this.tenants().filter((t) => t.isActive).length);
  readonly inactiveCount = computed(() => this.tenants().filter((t) => !t.isActive).length);
  readonly totalMembers = computed(() =>
    this.tenants().reduce((sum, t) => sum + t.memberCount, 0),
  );

  ngOnInit(): void {
    this.adminService.listTenants().subscribe({
      next: (tenants) => {
        this.tenants.set(tenants);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  openForm(): void {
    this.formOpen.set(true);
  }

  closeForm(): void {
    this.formOpen.set(false);
  }

  onSaved(tenant: AdminTenantDto): void {
    this.tenants.update((list) => [tenant, ...list]);
    this.formOpen.set(false);
  }

  toggleStatus(tenant: AdminTenantDto): void {
    const newStatus = !tenant.isActive;
    this.adminService.setTenantStatus(tenant.id, newStatus).subscribe(() => {
      this.tenants.update((list) =>
        list.map((t) => (t.id === tenant.id ? { ...t, isActive: newStatus } : t)),
      );
    });
  }

  getInitials(name: string): string {
    return name
      .split(' ')
      .slice(0, 2)
      .map((w) => w[0])
      .join('')
      .toUpperCase();
  }
}
