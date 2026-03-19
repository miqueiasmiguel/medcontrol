import { TestBed } from '@angular/core/testing';
import { WINDOW } from '../../core/tokens/window.token';
import { ThemeService } from './theme.service';

function createMockWindow(storedTheme: string | null = null) {
  const storage: Record<string, string> = {};
  if (storedTheme !== null) {
    storage['mmc_theme'] = storedTheme;
  }
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

describe('ThemeService', () => {
  function setup(storedTheme: string | null = null) {
    const mockWindow = createMockWindow(storedTheme);
    TestBed.configureTestingModule({
      providers: [ThemeService, { provide: WINDOW, useValue: mockWindow }],
    });
    return { service: TestBed.inject(ThemeService), mockWindow };
  }

  it('should default to "system" when no stored theme', () => {
    const { service } = setup();
    expect(service.theme()).toBe('system');
  });

  it('should restore stored "dark" theme', () => {
    const { service } = setup('dark');
    expect(service.theme()).toBe('dark');
  });

  it('should restore stored "light" theme', () => {
    const { service } = setup('light');
    expect(service.theme()).toBe('light');
  });

  it('should ignore invalid stored value and default to "system"', () => {
    const { service } = setup('invalid');
    expect(service.theme()).toBe('system');
  });

  // Testes de aplicação no DOM ao inicializar (regressão: tema voltava ao escuro no reload)

  it('init with stored "dark" should set data-theme="dark" on the DOM immediately', () => {
    const { mockWindow } = setup('dark');
    expect(mockWindow.document.documentElement.getAttribute('data-theme')).toBe('dark');
  });

  it('init with stored "light" should set data-theme="light" on the DOM immediately', () => {
    const { mockWindow } = setup('light');
    expect(mockWindow.document.documentElement.getAttribute('data-theme')).toBe('light');
  });

  it('init with no stored theme should not set data-theme on the DOM', () => {
    const { mockWindow } = setup();
    expect(mockWindow.document.documentElement.hasAttribute('data-theme')).toBe(false);
  });

  it('apply("dark") should set data-theme="dark" and update signal', () => {
    const { service, mockWindow } = setup();
    service.apply('dark');
    expect(mockWindow.document.documentElement.getAttribute('data-theme')).toBe('dark');
    expect(service.theme()).toBe('dark');
  });

  it('apply("light") should set data-theme="light" and update signal', () => {
    const { service, mockWindow } = setup();
    service.apply('light');
    expect(mockWindow.document.documentElement.getAttribute('data-theme')).toBe('light');
    expect(service.theme()).toBe('light');
  });

  it('apply("system") should remove data-theme attribute and update signal', () => {
    const { service, mockWindow } = setup('dark');
    service.apply('dark'); // set first
    service.apply('system');
    expect(mockWindow.document.documentElement.hasAttribute('data-theme')).toBe(false);
    expect(service.theme()).toBe('system');
  });

  it('apply should persist choice to localStorage', () => {
    const { service, mockWindow } = setup();
    service.apply('dark');
    expect(mockWindow.localStorage.getItem('mmc_theme')).toBe('dark');
  });
});
