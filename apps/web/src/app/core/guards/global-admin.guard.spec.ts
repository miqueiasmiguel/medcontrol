import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { globalAdminGuard } from './global-admin.guard';
import { CurrentUserService } from '../data-access/current-user.service';
import { UserDto } from '../../settings/data-access/settings.service';

const adminUser: UserDto = {
  id: 'admin-1',
  email: 'admin@test.com',
  displayName: 'Admin',
  avatarUrl: null,
  isEmailVerified: true,
  globalRole: 'Admin',
  lastLoginAt: null,
  tenantRole: null,
};

const regularUser: UserDto = { ...adminUser, globalRole: 'None', tenantRole: 'operator' };

describe('globalAdminGuard', () => {
  let currentUserService: { getMe: jest.Mock };
  let router: Router;

  function setup() {
    currentUserService = { getMe: jest.fn() };

    TestBed.configureTestingModule({
      providers: [
        provideRouter([{ path: '**', component: {} as any }]),
        { provide: CurrentUserService, useValue: currentUserService },
      ],
    });

    router = TestBed.inject(Router);
  }

  it('returns true for global admin', (done) => {
    setup();
    currentUserService.getMe.mockReturnValue(of(adminUser));

    TestBed.runInInjectionContext(() => {
      const result = globalAdminGuard({} as any, {} as any);
      (result as any).subscribe((val: any) => {
        expect(val).toBe(true);
        done();
      });
    });
  });

  it('redirects to / for non-admin', (done) => {
    setup();
    currentUserService.getMe.mockReturnValue(of(regularUser));
    const spy = jest.spyOn(router, 'createUrlTree');

    TestBed.runInInjectionContext(() => {
      const result = globalAdminGuard({} as any, {} as any);
      (result as any).subscribe(() => {
        expect(spy).toHaveBeenCalledWith(['/']);
        done();
      });
    });
  });

  it('redirects to / on error', (done) => {
    setup();
    currentUserService.getMe.mockReturnValue(throwError(() => new Error('fail')));
    const spy = jest.spyOn(router, 'createUrlTree');

    TestBed.runInInjectionContext(() => {
      const result = globalAdminGuard({} as any, {} as any);
      (result as any).subscribe(() => {
        expect(spy).toHaveBeenCalledWith(['/']);
        done();
      });
    });
  });
});
