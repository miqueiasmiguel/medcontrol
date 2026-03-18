import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { tenantGuard } from './tenant.guard';
import { TenantService, TenantDto } from '../data-access/tenant.service';

describe('tenantGuard', () => {
  let tenantService: jest.Mocked<Pick<TenantService, 'getMyTenants' | 'switchTenant'>>;
  let router: { createUrlTree: jest.Mock };

  function makeTenant(id: string): TenantDto {
    return { id, name: 'Clinic', slug: 'clinic', role: 'owner' };
  }

  beforeEach(() => {
    tenantService = {
      getMyTenants: jest.fn(),
      switchTenant: jest.fn(),
    };
    router = { createUrlTree: jest.fn((path) => path) };

    TestBed.configureTestingModule({
      providers: [
        { provide: TenantService, useValue: tenantService },
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
});
