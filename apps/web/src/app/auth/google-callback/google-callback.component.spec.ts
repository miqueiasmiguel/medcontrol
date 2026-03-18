import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter, Router, ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { GoogleCallbackComponent } from './google-callback.component';
import { AuthService } from '../data-access/auth.service';
import { WINDOW } from '../../core/tokens/window.token';

describe('GoogleCallbackComponent', () => {
  let authService: jest.Mocked<AuthService>;
  let mockWindow: { location: { href: string; origin: string } };
  let navigateSpy: jest.SpyInstance;

  function setup(queryParams: Record<string, string> = {}) {
    authService = {
      loginWithGoogle: jest.fn(),
      sendMagicLink: jest.fn(),
      verifyMagicLink: jest.fn(),
      logout: jest.fn(),
    } as unknown as jest.Mocked<AuthService>;

    mockWindow = { location: { href: '', origin: 'http://localhost:4200' } };

    TestBed.configureTestingModule({
      imports: [GoogleCallbackComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authService },
        { provide: WINDOW, useValue: mockWindow },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { queryParams } },
        },
      ],
    });

    navigateSpy = jest.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
  }

  it('should call loginWithGoogle with code from query params', fakeAsync(() => {
    setup({ code: 'test-code' });
    authService.loginWithGoogle.mockReturnValue(of(null));
    const fixture = TestBed.createComponent(GoogleCallbackComponent);
    fixture.detectChanges();
    tick();
    expect(authService.loginWithGoogle).toHaveBeenCalledWith('test-code', expect.any(String));
    expect(navigateSpy).toHaveBeenCalledWith(['/']);
  }));

  it('should redirect to /auth/login when no code in query params', fakeAsync(() => {
    setup({});
    const fixture = TestBed.createComponent(GoogleCallbackComponent);
    fixture.detectChanges();
    tick();
    expect(navigateSpy).toHaveBeenCalledWith(['/auth/login']);
  }));

  it('should redirect to /auth/login on error', fakeAsync(() => {
    setup({ code: 'bad-code' });
    authService.loginWithGoogle.mockReturnValue(throwError(() => new Error('auth failed')));
    const fixture = TestBed.createComponent(GoogleCallbackComponent);
    fixture.detectChanges();
    tick();
    expect(navigateSpy).toHaveBeenCalledWith(['/auth/login']);
  }));
});
