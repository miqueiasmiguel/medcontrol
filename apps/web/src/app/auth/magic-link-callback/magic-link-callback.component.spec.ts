import { DOCUMENT } from '@angular/common';
import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter, Router, ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { MagicLinkCallbackComponent } from './magic-link-callback.component';
import { AuthService } from '../data-access/auth.service';
import { WINDOW } from '../../core/tokens/window.token';

const IOS_UA = 'Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X)';
const ANDROID_UA = 'Mozilla/5.0 (Linux; Android 14; Pixel 8)';
const DESKTOP_UA = 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120';

describe('MagicLinkCallbackComponent', () => {
  let authService: jest.Mocked<AuthService>;
  let navigateSpy: jest.SpyInstance;
  let mockWindow: { location: { href: string }; navigator: { userAgent: string } };
  let mockVisibilityState: DocumentVisibilityState;
  let capturedVisibilityHandler: (() => void) | null;
  let addEventListenerSpy: jest.Mock;
  let removeEventListenerSpy: jest.Mock;

  function setup(queryParams: Record<string, string> = {}, userAgent = DESKTOP_UA) {
    capturedVisibilityHandler = null;
    mockVisibilityState = 'visible';
    mockWindow = { location: { href: '' }, navigator: { userAgent } };
    addEventListenerSpy = jest.fn((event: string, handler: () => void) => {
      if (event === 'visibilitychange') capturedVisibilityHandler = handler;
    });
    removeEventListenerSpy = jest.fn();

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

  // --- Desktop ---

  it('should redirect to /auth/login when no token', fakeAsync(() => {
    setup({});
    TestBed.createComponent(MagicLinkCallbackComponent).detectChanges();
    tick();
    expect(authService.verifyMagicLink).not.toHaveBeenCalled();
    expect(navigateSpy).toHaveBeenCalledWith(['/auth/login']);
  }));

  it('should call verifyMagicLink immediately on desktop (no trampoline)', fakeAsync(() => {
    setup({ token: 'abc123' });
    authService.verifyMagicLink.mockReturnValue(of(null));
    TestBed.createComponent(MagicLinkCallbackComponent).detectChanges();
    tick();
    expect(authService.verifyMagicLink).toHaveBeenCalledWith('abc123');
    expect(navigateSpy).toHaveBeenCalledWith(['/']);
  }));

  it('should redirect to /auth/login on desktop verifyMagicLink error', fakeAsync(() => {
    setup({ token: 'expired-token' });
    authService.verifyMagicLink.mockReturnValue(throwError(() => new Error('invalid')));
    TestBed.createComponent(MagicLinkCallbackComponent).detectChanges();
    tick();
    expect(navigateSpy).toHaveBeenCalledWith(['/auth/login']);
  }));

  it('should NOT set window.location.href on desktop', fakeAsync(() => {
    setup({ token: 'abc123' });
    authService.verifyMagicLink.mockReturnValue(of(null));
    TestBed.createComponent(MagicLinkCallbackComponent).detectChanges();
    expect(mockWindow.location.href).toBe('');
    tick();
  }));

  it('should NOT show appNotFound on desktop', fakeAsync(() => {
    setup({ token: 'abc123' });
    authService.verifyMagicLink.mockReturnValue(of(null));
    const fixture = TestBed.createComponent(MagicLinkCallbackComponent);
    fixture.detectChanges();
    expect(fixture.componentInstance.appNotFound()).toBe(false);
    tick();
  }));

  // --- Mobile: trampoline ---

  it('should set window.location.href to deep link on mobile', fakeAsync(() => {
    setup({ token: 'abc123' }, IOS_UA);
    TestBed.createComponent(MagicLinkCallbackComponent).detectChanges();
    expect(mockWindow.location.href).toBe('medcontrol://verify?token=abc123');
    expect(authService.verifyMagicLink).not.toHaveBeenCalled();
    tick(2500);
  }));

  it('should show tryingDeepLink while waiting on mobile', fakeAsync(() => {
    setup({ token: 'abc123' }, IOS_UA);
    const fixture = TestBed.createComponent(MagicLinkCallbackComponent);
    fixture.detectChanges();
    expect(fixture.componentInstance.tryingDeepLink()).toBe(true);
    tick(2500);
  }));

  it('should NOT call verifyMagicLink on mobile when app did not open', fakeAsync(() => {
    setup({ token: 'abc123' }, IOS_UA);
    TestBed.createComponent(MagicLinkCallbackComponent).detectChanges();
    tick(2500);
    expect(authService.verifyMagicLink).not.toHaveBeenCalled();
  }));

  it('should show appNotFound after timeout on iOS', fakeAsync(() => {
    setup({ token: 'abc123' }, IOS_UA);
    const fixture = TestBed.createComponent(MagicLinkCallbackComponent);
    fixture.detectChanges();
    tick(2500);
    fixture.detectChanges();
    expect(fixture.componentInstance.appNotFound()).toBe(true);
  }));

  it('should show appNotFound after timeout on Android', fakeAsync(() => {
    setup({ token: 'abc123' }, ANDROID_UA);
    const fixture = TestBed.createComponent(MagicLinkCallbackComponent);
    fixture.detectChanges();
    tick(2500);
    fixture.detectChanges();
    expect(fixture.componentInstance.appNotFound()).toBe(true);
  }));

  it('should NOT show appNotFound when app opens (visibilitychange fires)', fakeAsync(() => {
    setup({ token: 'abc123' }, IOS_UA);
    const fixture = TestBed.createComponent(MagicLinkCallbackComponent);
    fixture.detectChanges();
    mockVisibilityState = 'hidden';
    capturedVisibilityHandler?.();
    tick(3000);
    expect(fixture.componentInstance.appNotFound()).toBe(false);
    expect(authService.verifyMagicLink).not.toHaveBeenCalled();
  }));

  it('should detect iOS platform', fakeAsync(() => {
    setup({ token: 'abc123' }, IOS_UA);
    const fixture = TestBed.createComponent(MagicLinkCallbackComponent);
    fixture.detectChanges();
    expect(fixture.componentInstance.platform()).toBe('ios');
    tick(2500);
  }));

  it('should detect Android platform', fakeAsync(() => {
    setup({ token: 'abc123' }, ANDROID_UA);
    const fixture = TestBed.createComponent(MagicLinkCallbackComponent);
    fixture.detectChanges();
    expect(fixture.componentInstance.platform()).toBe('android');
    tick(2500);
  }));

  it('should clear timeout on ngOnDestroy', fakeAsync(() => {
    setup({ token: 'abc123' }, IOS_UA);
    const fixture = TestBed.createComponent(MagicLinkCallbackComponent);
    fixture.detectChanges();
    fixture.destroy();
    tick(2500);
    expect(authService.verifyMagicLink).not.toHaveBeenCalled();
  }));
});
