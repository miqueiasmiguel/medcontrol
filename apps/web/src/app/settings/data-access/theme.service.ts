import { Injectable, inject, signal } from '@angular/core';
import { WINDOW } from '../../core/tokens/window.token';

export type Theme = 'light' | 'dark' | 'system';

const STORAGE_KEY = 'mmc_theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly win = inject(WINDOW);
  readonly theme = signal<Theme>(this.loadStored());

  apply(theme: Theme): void {
    const root = this.win.document.documentElement;
    if (theme === 'dark') {
      root.setAttribute('data-theme', 'dark');
    } else if (theme === 'light') {
      root.setAttribute('data-theme', 'light');
    } else {
      root.removeAttribute('data-theme');
    }
    this.win.localStorage.setItem(STORAGE_KEY, theme);
    this.theme.set(theme);
  }

  private loadStored(): Theme {
    const stored = this.win.localStorage.getItem(STORAGE_KEY);
    if (stored === 'light' || stored === 'dark' || stored === 'system') {
      return stored;
    }
    return 'system';
  }
}
