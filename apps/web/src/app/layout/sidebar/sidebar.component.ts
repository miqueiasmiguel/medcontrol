import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  EventEmitter,
  Input,
  Output,
  inject,
} from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '../../auth/data-access/auth.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  template: `
    <nav class="sidebar" [class.sidebar--collapsed]="collapsed">
      <div class="sidebar__header">
        @if (!collapsed) {
          <span class="sidebar__brand">MedControl</span>
        }
        <button
          class="sidebar__toggle"
          (click)="toggleCollapse.emit()"
          aria-label="Alternar sidebar"
          type="button"
        >
          <span class="sidebar__toggle-icon">{{ collapsed ? '»' : '«' }}</span>
        </button>
      </div>

      <div class="sidebar__nav">
        <div class="sidebar__group">
          @if (!collapsed) {
            <span class="sidebar__group-label">Cadastros</span>
          }
          <a
            routerLink="/doctors"
            routerLinkActive="active"
            class="sidebar__item"
            [attr.aria-label]="collapsed ? 'Médicos' : undefined"
          >
            <svg class="sidebar__icon" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
              <path d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" />
            </svg>
            @if (!collapsed) {
              <span>Médicos</span>
            }
          </a>
          <a
            routerLink="/health-plans"
            routerLinkActive="active"
            class="sidebar__item"
            [attr.aria-label]="collapsed ? 'Convênios' : undefined"
          >
            <svg class="sidebar__icon" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
              <path
                fill-rule="evenodd"
                d="M3.172 5.172a4 4 0 015.656 0L10 6.343l1.172-1.171a4 4 0 115.656 5.656L10 17.657l-6.828-6.829a4 4 0 010-5.656z"
                clip-rule="evenodd"
              />
            </svg>
            @if (!collapsed) {
              <span>Convênios</span>
            }
          </a>
          <a
            routerLink="/procedures"
            routerLinkActive="active"
            class="sidebar__item"
            [attr.aria-label]="collapsed ? 'Procedimentos' : undefined"
          >
            <svg class="sidebar__icon" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
              <path
                fill-rule="evenodd"
                d="M6 2a2 2 0 00-2 2v12a2 2 0 002 2h8a2 2 0 002-2V7.414A2 2 0 0015.414 6L12 2.586A2 2 0 0010.586 2H6zm2 10a1 1 0 10-2 0v2a1 1 0 102 0v-2zm2-3a1 1 0 011 1v4a1 1 0 11-2 0V10a1 1 0 011-1zm2 0a1 1 0 10-2 0v4a1 1 0 102 0v-4z"
                clip-rule="evenodd"
              />
            </svg>
            @if (!collapsed) {
              <span>Procedimentos</span>
            }
          </a>
        </div>

        <div class="sidebar__group">
          @if (!collapsed) {
            <span class="sidebar__group-label">Operações</span>
          }
          <a
            routerLink="/payments"
            routerLinkActive="active"
            class="sidebar__item"
            [attr.aria-label]="collapsed ? 'Pagamentos' : undefined"
          >
            <svg class="sidebar__icon" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
              <path d="M4 4a2 2 0 00-2 2v1h16V6a2 2 0 00-2-2H4z" />
              <path
                fill-rule="evenodd"
                d="M18 9H2v5a2 2 0 002 2h12a2 2 0 002-2V9zM4 13a1 1 0 011-1h1a1 1 0 110 2H5a1 1 0 01-1-1zm5-1a1 1 0 100 2h1a1 1 0 100-2H9z"
                clip-rule="evenodd"
              />
            </svg>
            @if (!collapsed) {
              <span>Pagamentos</span>
            }
          </a>
        </div>

        <div class="sidebar__group">
          @if (!collapsed) {
            <span class="sidebar__group-label">Admin</span>
          }
          <a
            routerLink="/settings"
            routerLinkActive="active"
            class="sidebar__item"
            [attr.aria-label]="collapsed ? 'Configurações' : undefined"
          >
            <svg class="sidebar__icon" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
              <path
                fill-rule="evenodd"
                d="M11.49 3.17c-.38-1.56-2.6-1.56-2.98 0a1.532 1.532 0 01-2.286.948c-1.372-.836-2.942.734-2.106 2.106.54.886.061 2.042-.947 2.287-1.561.379-1.561 2.6 0 2.978a1.532 1.532 0 01.947 2.287c-.836 1.372.734 2.942 2.106 2.106a1.532 1.532 0 012.287.947c.379 1.561 2.6 1.561 2.978 0a1.533 1.533 0 012.287-.947c1.372.836 2.942-.734 2.106-2.106a1.533 1.533 0 01.947-2.287c1.561-.379 1.561-2.6 0-2.978a1.532 1.532 0 01-.947-2.287c.836-1.372-.734-2.942-2.106-2.106a1.532 1.532 0 01-2.287-.947zM10 13a3 3 0 100-6 3 3 0 000 6z"
                clip-rule="evenodd"
              />
            </svg>
            @if (!collapsed) {
              <span>Configurações</span>
            }
          </a>
          <a
            routerLink="/members"
            routerLinkActive="active"
            class="sidebar__item"
            [attr.aria-label]="collapsed ? 'Membros' : undefined"
          >
            <svg class="sidebar__icon" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
              <path d="M9 6a3 3 0 11-6 0 3 3 0 016 0zM17 6a3 3 0 11-6 0 3 3 0 016 0zM12.93 17c.046-.327.07-.66.07-1a6.97 6.97 0 00-1.5-4.33A5 5 0 0119 16v1h-6.07zM6 11a5 5 0 015 5v1H1v-1a5 5 0 015-5z" />
            </svg>
            @if (!collapsed) {
              <span>Membros</span>
            }
          </a>
        </div>
      </div>

      <div class="sidebar__footer">
        <button class="sidebar__logout" (click)="logout()" type="button">
          <svg class="sidebar__icon" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
            <path
              fill-rule="evenodd"
              d="M3 3a1 1 0 00-1 1v12a1 1 0 102 0V4a1 1 0 00-1-1zm10.293 9.293a1 1 0 001.414 1.414l3-3a1 1 0 000-1.414l-3-3a1 1 0 10-1.414 1.414L14.586 9H7a1 1 0 100 2h7.586l-1.293 1.293z"
              clip-rule="evenodd"
            />
          </svg>
          @if (!collapsed) {
            <span>Sair</span>
          }
        </button>
      </div>
    </nav>
  `,
  styleUrl: './sidebar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SidebarComponent {
  @Input() collapsed = false;
  @Output() readonly toggleCollapse = new EventEmitter<void>();

  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  logout() {
    this.authService
      .logout()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.router.navigate(['/auth/login']),
      });
  }
}
