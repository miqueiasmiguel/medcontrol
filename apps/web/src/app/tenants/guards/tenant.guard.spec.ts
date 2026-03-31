import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { tenantGuard } from './tenant.guard';
import { TenantService, TenantDto } from '../data-access/tenant.service';
import { CurrentUserService } from '../../core/data-access/current-user.service';
import { UserDto } from '../../settings/data-access/settings.service';

const noTenantUser: UserDto = {
  id: 'user-1',
  email: 'user@test.com',
  displayName: 'User',
  avatarUrl: null,
  isEmailVerified: true,
  globalRole: 'None',
  lastLoginAt: null,
  tenantRole: null,
};

const regularUser: UserDto = { ...noTenantUser, tenantRole: 'operator' };

const adminUser: UserDto = { ...noTenantUser, globalRole: 'Admin' };

describe('tenantGuard', () => {
  let tenantService: jest.Mocked<Pick<TenantService, 'getMyTenants' | 'switchTenant'>>;
  let currentUserService: { getMe: jest.Mock; invalidate: jest.Mock };
  let router: { createUrlTree: jest.Mock };

  function makeTenant(id: string): TenantDto {
    return { id, name: 'Clinic', slug: 'clinic', role: 'owner' };
  }

  beforeEach(() => {
    tenantService = {
      getMyTenants: jest.fn(),
      switchTenant: jest.fn(),
    };
    currentUserService = { getMe: jest.fn().mockReturnValue(of(regularUser)), invalidate: jest.fn() };
    router = { createUrlTree: jest.fn((path) => path) };

    TestBed.configureTestingModule({
      providers: [
        { provide: TenantService, useValue: tenantService },
        { provide: CurrentUserService, useValue: currentUserService },
        { provide: Router, useValue: router },
      ],
    });
  });

  it('0 tenants → redirects to /tenants/new', fakeAsync(() => {
    tenantService.getMyTenants.mockReturnValue(of([]));
    let result: unknown;

    TestBed.runInInjectionContext(() => {
      (tenantGuard({} as never, {} as never) as ReturnType<typeof of>).subscribe((r) => {
        result = r;
      });
    });
    tick();

    expect(router.createUrlTree).toHaveBeenCalledWith(['/tenants/new']);
    expect(result).toEqual(['/tenants/new']);
  }));

  it('0 tenants + global admin → redirects to /admin', fakeAsync(() => {
    currentUserService.getMe.mockReturnValue(of(adminUser));
    tenantService.getMyTenants.mockReturnValue(of([]));
    let result: unknown;

    TestBed.runInInjectionContext(() => {
      (tenantGuard({} as never, {} as never) as ReturnType<typeof of>).subscribe((r) => {
        result = r;
      });
    });
    tick();

    expect(router.createUrlTree).toHaveBeenCalledWith(['/admin']);
    expect(result).toEqual(['/admin']);
  }));

  it('1 tenant → calls switchTenant and returns true', fakeAsync(() => {
    tenantService.getMyTenants.mockReturnValue(of([makeTenant('t1')]));
    tenantService.switchTenant.mockReturnValue(of(null));
    let result: unknown;

    TestBed.runInInjectionContext(() => {
      (tenantGuard({} as never, {} as never) as ReturnType<typeof of>).subscribe((r) => {
        result = r;
      });
    });
    tick();

    expect(tenantService.switchTenant).toHaveBeenCalledWith('t1');
    expect(result).toBe(true);
  }));

  it('1 tenant → after switchTenant, invalidates cache and refetches user so role is fresh', fakeAsync(() => {
    const userWithoutRole: UserDto = { ...noTenantUser, tenantRole: null };
    const userWithRole: UserDto = { ...noTenantUser, tenantRole: 'doctor' };
    currentUserService.getMe
      .mockReturnValueOnce(of(userWithoutRole))
      .mockReturnValueOnce(of(userWithRole));
    tenantService.getMyTenants.mockReturnValue(of([makeTenant('t1')]));
    tenantService.switchTenant.mockReturnValue(of(null));
    let result: unknown;

    TestBed.runInInjectionContext(() => {
      (tenantGuard({} as never, {} as never) as ReturnType<typeof of>).subscribe((r) => {
        result = r;
      });
    });
    tick();

    expect(currentUserService.invalidate).toHaveBeenCalled();
    expect(currentUserService.getMe).toHaveBeenCalledTimes(2);
    expect(result).toBe(true);
  }));

  it('2+ tenants, no tenant selected yet → redirects to /tenants/select', fakeAsync(() => {
    currentUserService.getMe.mockReturnValue(of(noTenantUser));
    tenantService.getMyTenants.mockReturnValue(of([makeTenant('t1'), makeTenant('t2')]));
    let result: unknown;

    TestBed.runInInjectionContext(() => {
      (tenantGuard({} as never, {} as never) as ReturnType<typeof of>).subscribe((r) => {
        result = r;
      });
    });
    tick();

    expect(router.createUrlTree).toHaveBeenCalledWith(['/tenants/select']);
    expect(result).toEqual(['/tenants/select']);
  }));

  it('2+ tenants, tenant already selected in JWT → allows navigation', fakeAsync(() => {
    currentUserService.getMe.mockReturnValue(of(regularUser));
    tenantService.getMyTenants.mockReturnValue(of([makeTenant('t1'), makeTenant('t2')]));
    let result: unknown;

    TestBed.runInInjectionContext(() => {
      (tenantGuard({} as never, {} as never) as ReturnType<typeof of>).subscribe((r) => {
        result = r;
      });
    });
    tick();

    expect(router.createUrlTree).not.toHaveBeenCalledWith(['/tenants/select']);
    expect(result).toBe(true);
  }));

  it('getMyTenants error → redirects to /tenants/new', fakeAsync(() => {
    tenantService.getMyTenants.mockReturnValue(throwError(() => new Error('Network error')));
    let result: unknown;

    TestBed.runInInjectionContext(() => {
      (tenantGuard({} as never, {} as never) as ReturnType<typeof of>).subscribe((r) => {
        result = r;
      });
    });
    tick();

    expect(router.createUrlTree).toHaveBeenCalledWith(['/tenants/new']);
    expect(result).toEqual(['/tenants/new']);
  }));

  it('getMyTenants 401 → redirects to /auth/login', fakeAsync(() => {
    tenantService.getMyTenants.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 401 })),
    );
    let result: unknown;

    TestBed.runInInjectionContext(() => {
      (tenantGuard({} as never, {} as never) as ReturnType<typeof of>).subscribe((r) => {
        result = r;
      });
    });
    tick();

    expect(router.createUrlTree).toHaveBeenCalledWith(['/auth/login']);
    expect(result).toEqual(['/auth/login']);
  }));
});
