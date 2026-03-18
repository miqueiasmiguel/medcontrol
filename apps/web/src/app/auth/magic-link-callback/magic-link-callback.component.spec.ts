import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter, Router, ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { MagicLinkCallbackComponent } from './magic-link-callback.component';
import { AuthService } from '../data-access/auth.service';

describe('MagicLinkCallbackComponent', () => {
  let authService: jest.Mocked<AuthService>;
  let navigateSpy: jest.SpyInstance;

  function setup(queryParams: Record<string, string> = {}) {
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
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { queryParams } },
        },
      ],
    });

    navigateSpy = jest.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
  }

  it('should call verifyMagicLink with token from query params', fakeAsync(() => {
    setup({ token: 'abc123' });
    authService.verifyMagicLink.mockReturnValue(of(null));
    const fixture = TestBed.createComponent(MagicLinkCallbackComponent);
    fixture.detectChanges();
    tick();
    expect(authService.verifyMagicLink).toHaveBeenCalledWith('abc123');
    expect(navigateSpy).toHaveBeenCalledWith(['/']);
  }));

  it('should redirect to /auth/login when no token in query params', fakeAsync(() => {
    setup({});
    const fixture = TestBed.createComponent(MagicLinkCallbackComponent);
    fixture.detectChanges();
    tick();
    expect(authService.verifyMagicLink).not.toHaveBeenCalled();
    expect(navigateSpy).toHaveBeenCalledWith(['/auth/login']);
  }));

  it('should redirect to /auth/login on error', fakeAsync(() => {
    setup({ token: 'expired-token' });
    authService.verifyMagicLink.mockReturnValue(throwError(() => new Error('invalid token')));
    const fixture = TestBed.createComponent(MagicLinkCallbackComponent);
    fixture.detectChanges();
    tick();
    expect(navigateSpy).toHaveBeenCalledWith(['/auth/login']);
  }));
});
