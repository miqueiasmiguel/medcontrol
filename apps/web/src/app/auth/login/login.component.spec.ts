import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Subject, of, throwError } from 'rxjs';
import { LoginComponent } from './login.component';
import { AuthService } from '../data-access/auth.service';
import { WINDOW } from '../../core/tokens/window.token';

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
});
