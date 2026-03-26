import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { roleGuard } from './role.guard';
import { CurrentUserService } from '../data-access/current-user.service';
import { UserDto } from '../../settings/data-access/settings.service';

const operatorUser: UserDto = {
  id: 'user-1',
  email: 'operator@test.com',
  displayName: 'Operator',
  avatarUrl: null,
  isEmailVerified: true,
  globalRole: 'None',
  lastLoginAt: null,
  tenantRole: 'operator',
};

const doctorUser: UserDto = { ...operatorUser, tenantRole: 'doctor' };

describe('roleGuard', () => {
  let currentUserService: { getMe: jest.Mock };

  function setup() {
    currentUserService = { getMe: jest.fn() };

    TestBed.configureTestingModule({
      providers: [
        provideRouter([
          { path: 'payments', children: [] },
          { path: 'doctors', children: [] },
          { path: 'health-plans', children: [] },
          { path: 'procedures', children: [] },
          { path: 'members', children: [] },
        ]),
        { provide: CurrentUserService, useValue: currentUserService },
      ],
    });
  }

  function runGuard(): Promise<boolean | import('@angular/router').UrlTree> {
    return TestBed.runInInjectionContext(() => {
      const result = roleGuard({} as never, {} as never);
      if (typeof result === 'object' && 'subscribe' in result) {
        return new Promise((resolve) =>
          result.subscribe((v) => resolve(v as boolean | import('@angular/router').UrlTree)),
        );
      }
      return Promise.resolve(result as boolean | import('@angular/router').UrlTree);
    });
  }

  it('allows access when user is not a doctor', async () => {
    setup();
    currentUserService.getMe.mockReturnValue(of(operatorUser));

    const result = await runGuard();
    expect(result).toBe(true);
  });

  it('redirects to /payments when user is a doctor', async () => {
    setup();
    currentUserService.getMe.mockReturnValue(of(doctorUser));

    const result = await runGuard();
    const router = TestBed.inject(Router);
    expect(result).toEqual(router.createUrlTree(['/payments']));
  });

  it('allows access on getMe error (failsafe)', async () => {
    setup();
    currentUserService.getMe.mockReturnValue(throwError(() => new Error('network')));

    const result = await runGuard();
    expect(result).toBe(true);
  });
});
