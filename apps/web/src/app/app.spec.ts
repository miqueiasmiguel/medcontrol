import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { WINDOW } from './core/tokens/window.token';
import { ThemeService } from './settings/data-access/theme.service';
import { App } from './app';

function createMockWindow(storedTheme: string | null = null) {
  const storage: Record<string, string> = {};
  if (storedTheme !== null) storage['mmc_theme'] = storedTheme;
  return {
    localStorage: {
      getItem: (key: string) => storage[key] ?? null,
      setItem: (key: string, value: string) => { storage[key] = value; },
    },
    document: {
      documentElement: {
        _attrs: {} as Record<string, string>,
        getAttribute(attr: string) { return this._attrs[attr] ?? null; },
        setAttribute(attr: string, value: string) { this._attrs[attr] = value; },
        removeAttribute(attr: string) { delete this._attrs[attr]; },
        hasAttribute(attr: string) { return attr in this._attrs; },
      },
    },
  };
}

describe('App', () => {
  it('should create the app', () => {
    TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([])],
    });
    const fixture = TestBed.createComponent(App);
    expect(fixture.componentInstance).toBeTruthy();
  });

  // Regressão: tema voltava ao escuro ao recarregar em rotas que não usam SettingsComponent,
  // porque ThemeService era instanciado de forma lazy (apenas quando SettingsComponent era carregado).
  it('should apply stored theme on startup without visiting settings page', () => {
    const mockWindow = createMockWindow('light');
    TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter([]),
        ThemeService,
        { provide: WINDOW, useValue: mockWindow },
      ],
    });

    TestBed.createComponent(App);

    expect(mockWindow.document.documentElement.getAttribute('data-theme')).toBe('light');
  });

  it('should apply stored dark theme on startup without visiting settings page', () => {
    const mockWindow = createMockWindow('dark');
    TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter([]),
        ThemeService,
        { provide: WINDOW, useValue: mockWindow },
      ],
    });

    TestBed.createComponent(App);

    expect(mockWindow.document.documentElement.getAttribute('data-theme')).toBe('dark');
  });
});
