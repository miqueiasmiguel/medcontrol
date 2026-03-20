import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Subject, of, throwError } from 'rxjs';
import { LoginComponent } from './login.component';
import { AuthService } from '../data-access/auth.service';
import { WINDOW } from '../../core/tokens/window.token';
import { GOOGLE_CLIENT_ID } from '../../core/tokens/google-client-id.token';
import { GOOGLE_REDIRECT_URI } from '../../core/tokens/google-redirect-uri.token';

describe('LoginComponent', () => {
  let authService: jest.Mocked<AuthService>;
  let mockWindow: { location: { href: string; origin: string } };
  let navigateSpy: jest.SpyInstance;

  beforeEach(() => {
    authService = {
      sendMagicLink: jest.fn(),
      verifyMagicLink: jest.fn(),
      loginWithGoogle: jest.fn(),
      logout: jest.fn(),
    } as unknown as jest.Mocked<AuthService>;

    mockWindow = { location: { href: '', origin: 'http://localhost:4200' } };

    TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideRouter([]),
        provideNoopAnimations(),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: authService },
        { provide: WINDOW, useValue: mockWindow },
        { provide: GOOGLE_CLIENT_ID, useValue: 'test-client-id' },
      ],
    });

    navigateSpy = jest.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
  });

  function createComponent() {
    const fixture = TestBed.createComponent(LoginComponent);
    fixture.detectChanges();
    return { fixture, el: fixture.nativeElement as HTMLElement };
  }

  it('should render email input and submit button', () => {
    const { el } = createComponent();
    expect(el.querySelector('input[type="email"]')).toBeTruthy();
    expect(el.querySelector('button[type="submit"]')).toBeTruthy();
  });

  it('should show required error when submitting empty form', fakeAsync(() => {
    const { fixture, el } = createComponent();
    el.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
    fixture.detectChanges();
    tick();
    fixture.detectChanges();
    expect(el.textContent).toContain('E-mail é obrigatório');
  }));

  it('should show format error for invalid email', fakeAsync(() => {
    const { fixture, el } = createComponent();
    const input = el.querySelector<HTMLInputElement>('input[type="email"]')!;
    input.value = 'not-an-email';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    el.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
    tick();
    fixture.detectChanges();
    expect(el.textContent).toContain('E-mail inválido');
  }));

  it('should call AuthService.sendMagicLink with form value', fakeAsync(() => {
    authService.sendMagicLink.mockReturnValue(of(null));
    const { fixture, el } = createComponent();
    const input = el.querySelector<HTMLInputElement>('input[type="email"]')!;
    input.value = 'user@test.com';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    el.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
    tick();
    expect(authService.sendMagicLink).toHaveBeenCalledWith('user@test.com');
  }));

  it('should navigate to /auth/magic-link-sent after success', fakeAsync(() => {
    authService.sendMagicLink.mockReturnValue(of(null));
    const { fixture, el } = createComponent();
    const input = el.querySelector<HTMLInputElement>('input[type="email"]')!;
    input.value = 'user@test.com';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    el.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
    tick();
    expect(navigateSpy).toHaveBeenCalledWith(['/auth/magic-link-sent'], expect.any(Object));
  }));

  it('should set errorMessage signal on network error', fakeAsync(() => {
    authService.sendMagicLink.mockReturnValue(throwError(() => new Error('Network error')));
    const { fixture, el } = createComponent();
    const input = el.querySelector<HTMLInputElement>('input[type="email"]')!;
    input.value = 'user@test.com';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    el.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
    tick();
    fixture.detectChanges();
    expect(fixture.componentInstance.errorMessage()).toBeTruthy();
  }));

  it('should disable submit button while loading', fakeAsync(() => {
    const subject = new Subject<null>();
    authService.sendMagicLink.mockReturnValue(subject.asObservable());
    const { fixture, el } = createComponent();
    const input = el.querySelector<HTMLInputElement>('input[type="email"]')!;
    input.value = 'user@test.com';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    el.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
    fixture.detectChanges();
    const btn = el.querySelector<HTMLButtonElement>('button[type="submit"]')!;
    expect(btn.disabled).toBe(true);
    subject.complete();
  }));

  it('should redirect to Google OAuth when clicking Google button', () => {
    const { el } = createComponent();
    const googleBtn = el.querySelector<HTMLButtonElement>('[data-testid="google-btn"]')!;
    googleBtn.click();
    expect(mockWindow.location.href).toContain('accounts.google.com');
  });

  it('should include client_id in Google OAuth redirect url', () => {
    const { el } = createComponent();
    const googleBtn = el.querySelector<HTMLButtonElement>('[data-testid="google-btn"]')!;
    googleBtn.click();
    expect(mockWindow.location.href).toContain('client_id=test-client-id');
  });

  it('should include redirect_uri with /auth/callback in Google OAuth redirect url', () => {
    const { el } = createComponent();
    const googleBtn = el.querySelector<HTMLButtonElement>('[data-testid="google-btn"]')!;
    googleBtn.click();
    expect(mockWindow.location.href).toContain(encodeURIComponent('http://localhost:4200/auth/callback'));
  });

  it('should use GOOGLE_REDIRECT_URI token as origin when provided', () => {
    mockWindow = { location: { href: '', origin: 'http://localhost:4200' } };
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideRouter([]),
        provideNoopAnimations(),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: authService },
        { provide: WINDOW, useValue: mockWindow },
        { provide: GOOGLE_CLIENT_ID, useValue: 'test-client-id' },
        { provide: GOOGLE_REDIRECT_URI, useValue: 'https://medcontrol-web.pages.dev' },
      ],
    });
    const fixture = TestBed.createComponent(LoginComponent);
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    el.querySelector<HTMLButtonElement>('[data-testid="google-btn"]')!.click();
    expect(mockWindow.location.href).toContain(
      encodeURIComponent('https://medcontrol-web.pages.dev/auth/callback'),
    );
    expect(mockWindow.location.href).not.toContain(encodeURIComponent('http://localhost:4200'));
  });
});
