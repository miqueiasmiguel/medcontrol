import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { Location } from '@angular/common';
import { of } from 'rxjs';
import { appRoutes } from './app.routes';
import { SessionService } from './auth/data-access/session.service';
import { TenantService } from './tenants/data-access/tenant.service';

describe('appRoutes', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter(appRoutes),
        {
          provide: SessionService,
          useValue: { isAuthenticated: () => false },
        },
        {
          provide: TenantService,
          useValue: { getMyTenants: () => of([]), switchTenant: () => of(null) },
        },
      ],
    });
  });

  it('should redirect / to /auth/login', fakeAsync(() => {
    const router = TestBed.inject(Router);
    const location = TestBed.inject(Location);
    router.navigate(['/']).catch(() => void 0);
    tick();
    expect(location.path()).toBe('/auth/login');
  }));

  it('should redirect /tenants/new to /auth/login when not authenticated', fakeAsync(() => {
    const router = TestBed.inject(Router);
    const location = TestBed.inject(Location);
    router.navigate(['/tenants/new']).catch(() => void 0);
    tick();
    expect(location.path()).toBe('/auth/login');
  }));

  it('should redirect /tenants/select to /auth/login when not authenticated', fakeAsync(() => {
    const router = TestBed.inject(Router);
    const location = TestBed.inject(Location);
    router.navigate(['/tenants/select']).catch(() => void 0);
    tick();
    expect(location.path()).toBe('/auth/login');
  }));
});
