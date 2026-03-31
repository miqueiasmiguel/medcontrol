import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';

@Component({
  selector: 'app-admin-shell',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterOutlet, RouterLink],
  template: `
    <div class="as-page">

      <header class="as-header">
        <div class="as-header__brand">
          <div class="as-logo">
            <span class="as-logo__letter">M</span>
          </div>
          <div class="as-brand-info">
            <span class="as-wordmark">Med<span class="as-wordmark__accent">Control</span></span>
            <span class="as-admin-badge">
              <svg width="10" height="10" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
                <path d="M12 1L3 5v6c0 5.55 3.84 10.74 9 12 5.16-1.26 9-6.45 9-12V5L12 1z"/>
              </svg>
              Admin Global
            </span>
          </div>
        </div>

        <nav class="as-nav">
          <a class="as-nav__item" routerLink="/admin" routerLinkActive="as-nav__item--active">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
              <rect x="2" y="7" width="20" height="14" rx="2"/>
              <path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/>
            </svg>
            Organizações
          </a>
        </nav>

        <div class="as-header__end">
          <div class="as-access-chip">
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
              <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/>
              <path d="M7 11V7a5 5 0 0 1 10 0v4"/>
            </svg>
            Acesso Restrito
          </div>
        </div>
      </header>

      <main class="as-main">
        <router-outlet />
      </main>

    </div>
  `,
  styles: [`
    :host {
      display: block;
    }

    .as-page {
      min-height: 100vh;
      background: var(--mmc-bg);
      display: flex;
      flex-direction: column;
    }

    // ── Header ──────────────────────────────────────────────────────────────────

    .as-header {
      height: 60px;
      background: var(--mmc-navy-950);
      display: flex;
      align-items: center;
      padding: 0 var(--mmc-space-6);
      gap: var(--mmc-space-8);
      position: sticky;
      top: 0;
      z-index: 50;
      box-shadow: 0 1px 0 rgba(255, 255, 255, 0.06);

      &__brand {
        display: flex;
        align-items: center;
        gap: var(--mmc-space-3);
        flex-shrink: 0;
      }

      &__end {
        margin-left: auto;
      }
    }

    .as-logo {
      width: 34px;
      height: 34px;
      border-radius: var(--mmc-radius-md);
      background: var(--mmc-orange-500);
      display: flex;
      align-items: center;
      justify-content: center;
      box-shadow: 0 4px 12px rgba(249, 115, 22, 0.4);
      flex-shrink: 0;

      &__letter {
        font-size: 17px;
        font-weight: 800;
        color: var(--mmc-neutral-0);
        line-height: 1;
        letter-spacing: -0.5px;
      }
    }

    .as-brand-info {
      display: flex;
      flex-direction: column;
      gap: 2px;
    }

    .as-wordmark {
      font-size: var(--mmc-text-sm);
      font-weight: 700;
      color: var(--mmc-neutral-0);
      letter-spacing: 0.2px;
      line-height: 1;

      &__accent {
        color: var(--mmc-orange-400);
      }
    }

    .as-admin-badge {
      display: inline-flex;
      align-items: center;
      gap: 4px;
      font-size: 9px;
      font-weight: 600;
      letter-spacing: 0.6px;
      text-transform: uppercase;
      color: var(--mmc-orange-400);
      line-height: 1;
    }

    // ── Nav ─────────────────────────────────────────────────────────────────────

    .as-nav {
      display: flex;
      align-items: center;
      gap: var(--mmc-space-1);
      height: 100%;

      &__item {
        display: inline-flex;
        align-items: center;
        gap: var(--mmc-space-2);
        height: 36px;
        padding: 0 var(--mmc-space-3);
        font-size: var(--mmc-text-sm);
        font-weight: var(--mmc-font-weight-medium);
        color: rgba(255, 255, 255, 0.55);
        border-radius: var(--mmc-radius-md);
        text-decoration: none;
        transition: color 150ms ease, background 150ms ease;

        svg {
          flex-shrink: 0;
          opacity: 0.7;
          transition: opacity 150ms ease;
        }

        &:hover {
          color: rgba(255, 255, 255, 0.85);
          background: rgba(255, 255, 255, 0.06);

          svg { opacity: 1; }
        }

        &--active {
          color: var(--mmc-neutral-0);
          background: rgba(255, 255, 255, 0.10);

          svg { opacity: 1; }
        }
      }
    }

    // ── Access chip ─────────────────────────────────────────────────────────────

    .as-access-chip {
      display: inline-flex;
      align-items: center;
      gap: var(--mmc-space-1-5);
      height: 28px;
      padding: 0 var(--mmc-space-3);
      background: rgba(249, 115, 22, 0.12);
      border: 1px solid rgba(249, 115, 22, 0.25);
      border-radius: var(--mmc-radius-full);
      font-size: var(--mmc-text-xs);
      font-weight: var(--mmc-font-weight-medium);
      color: var(--mmc-orange-400);
    }

    // ── Main ────────────────────────────────────────────────────────────────────

    .as-main {
      flex: 1;
      display: flex;
      flex-direction: column;
    }
  `],
})
export class AdminShellComponent {}
