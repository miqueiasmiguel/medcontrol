import { DOCUMENT } from '@angular/common';
import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter, Router, ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { MagicLinkCallbackComponent } from './magic-link-callback.component';
import { AuthService } from '../data-access/auth.service';
import { WINDOW } from '../../core/tokens/window.token';

describe('MagicLinkCallbackComponent', () => {
  let authService: jest.Mocked<AuthService>;
  let navigateSpy: jest.SpyInstance;
  let mockWindow: { location: { href: string } };
  let mockVisibilityState: DocumentVisibilityState;
  let capturedVisibilityHandler: (() => void) | null;
  let addEventListenerSpy: jest.Mock;
  let removeEventListenerSpy: jest.Mock;

  function setup(queryParams: Record<string, string> = {}) {
    capturedVisibilityHandler = null;
    mockVisibilityState = 'visible';
    mockWindow = { location: { href: '' } };
    addEventListenerSpy = jest.fn((event: string, handler: () => void) => {
      if (event === 'visibilitychange') capturedVisibilityHandler = handler;
    });
    removeEventListenerSpy = jest.fn();

    // Proxy wraps the real document so Angular internals (querySelectorAll etc.) still work.
    // Only visibilityState, addEventListener and removeEventListener are intercepted.
    const docProxy = new Proxy(document, {
      get(target, prop: string) {
        if (prop === 'visibilityState') return mockVisibilityState;
        if (prop === 'addEventListener') return addEventListenerSpy;
        if (prop === 'removeEventListener') return removeEventListenerSpy;
        const value = target[prop as keyof Document];
        return typeof value === 'function'
          ? (value as (...args: unknown[]) => unknown).bind(target)
          : value;
      },
    });

    authService = {
      loginWithGoogle: jest.fn(),
      sendMagicLink: jest.fn(),
      verifyMagicLink: jest.fn(),
      logout: jest.fn(),
    } as unknown as jest.Mocked<AuthService>;

    TestBed.configureTestingModule({
      imports: [MagicLinkCallbackComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authService },
        { provide: ActivatedRoute, useValue: { snapshot: { queryParams } } },
        { provide: WINDOW, useValue: mockWindow },
        { provide: DOCUMENT, useValue: docProxy },
      ],
    });

    navigateSpy = jest.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
  }

  // --- Testes existentes (atualizados para o trampoline) ---

  it('should redirect to /auth/login when no token in query params', fakeAsync(() => {
    setup({});
    TestBed.createComponent(MagicLinkCallbackComponent).detectChanges();
    tick();
    expect(authService.verifyMagicLink).not.toHaveBeenCalled();
    expect(navigateSpy).toHaveBeenCalledWith(['/auth/login']);
  }));

  it('should call verifyMagicLink and navigate to / after 2500ms fallback', fakeAsync(() => {
    setup({ token: 'abc123' });
    authService.verifyMagicLink.mockReturnValue(of(null));
    TestBed.createComponent(MagicLinkCallbackComponent).detectChanges();
    tick(2500);
    expect(authService.verifyMagicLink).toHaveBeenCalledWith('abc123');
    expect(navigateSpy).toHaveBeenCalledWith(['/']);
  }));

  it('should redirect to /auth/login on error after 2500ms fallback', fakeAsync(() => {
    setup({ token: 'expired-token' });
    authService.verifyMagicLink.mockReturnValue(throwError(() => new Error('invalid token')));
    TestBed.createComponent(MagicLinkCallbackComponent).detectChanges();
    tick(2500);
    expect(navigateSpy).toHaveBeenCalledWith(['/auth/login']);
  }));

  // --- Novos testes do trampoline ---

  it('should set window.location.href to deep link immediately on load', fakeAsync(() => {
    setup({ token: 'abc123' });
    authService.verifyMagicLink.mockReturnValue(of(null));
    TestBed.createComponent(MagicLinkCallbackComponent).detectChanges();
    expect(mockWindow.location.href).toBe('medcontrol://verify?token=abc123');
    expect(authService.verifyMagicLink).not.toHaveBeenCalled();
    tick(2500); // cleanup timers
  }));

  it('should NOT call verifyMagicLink when visibilitychange fires (app opened)', fakeAsync(() => {
    setup({ token: 'abc123' });
    TestBed.createComponent(MagicLinkCallbackComponent).detectChanges();
    // Simulate app opening: document becomes hidden
    mockVisibilityState = 'hidden';
    capturedVisibilityHandler?.();
    tick(3000); // past the 2500ms timeout
    expect(authService.verifyMagicLink).not.toHaveBeenCalled();
  }));

  it('should call verifyMagicLink as web fallback after 2500ms when app did not open', fakeAsync(() => {
    setup({ token: 'abc123' });
    authService.verifyMagicLink.mockReturnValue(of(null));
    TestBed.createComponent(MagicLinkCallbackComponent).detectChanges();
    // Do NOT fire visibilitychange — app did not open
    tick(2500);
    expect(authService.verifyMagicLink).toHaveBeenCalledWith('abc123');
  }));

  it('should show tryingDeepLink as true while waiting for app to open', fakeAsync(() => {
    setup({ token: 'abc123' });
    authService.verifyMagicLink.mockReturnValue(of(null));
    const fixture = TestBed.createComponent(MagicLinkCallbackComponent);
    fixture.detectChanges();
    expect(fixture.componentInstance.tryingDeepLink()).toBe(true);
    tick(2500); // cleanup timers
  }));

  it('should clear timeout on ngOnDestroy (no verifyMagicLink after destroy)', fakeAsync(() => {
    setup({ token: 'abc123' });
    const fixture = TestBed.createComponent(MagicLinkCallbackComponent);
    fixture.detectChanges();
    fixture.destroy();
    tick(2500);
    expect(authService.verifyMagicLink).not.toHaveBeenCalled();
  }));
});
