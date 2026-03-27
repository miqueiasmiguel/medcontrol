import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { tenantGuard } from './tenant.guard';
import { TenantService, TenantDto } from '../data-access/tenant.service';
import { CurrentUserService } from '../../core/data-access/current-user.service';
import { UserDto } from '../../settings/data-access/settings.service';

const regularUser: UserDto = {
  id: 'user-1',
  email: 'user@test.com',
  displayName: 'User',
  avatarUrl: null,
  isEmailVerified: true,
  globalRole: 'None',
  lastLoginAt: null,
  tenantRole: 'operator',
};

const adminUser: UserDto = { ...regularUser, globalRole: 'Admin', tenantRole: null };

describe('tenantGuard', () => {
  let tenantService: jest.Mocked<Pick<TenantService, 'getMyTenants' | 'switchTenant'>>;
  let currentUserService: { getMe: jest.Mock };
  let router: { createUrlTree: jest.Mock };

  function makeTenant(id: string): TenantDto {
    return { id, name: 'Clinic', slug: 'clinic', role: 'owner' };
  }

  beforeEach(() => {
    tenantService = {
      getMyTenants: jest.fn(),
      switchTenant: jest.fn(),
    };
    currentUserService = { getMe: jest.fn().mockReturnValue(of(regularUser)) };
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

  it('2+ tenants → redirects to /tenants/select', fakeAsync(() => {
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
