import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { TenantSelectComponent } from './tenant-select.component';
import { TenantService, TenantDto } from '../data-access/tenant.service';
import { CurrentUserService } from '../../core/data-access/current-user.service';

describe('TenantSelectComponent', () => {
  let tenantService: jest.Mocked<Pick<TenantService, 'getMyTenants' | 'switchTenant'>>;
  let currentUserService: { invalidate: jest.Mock };
  let navigateSpy: jest.SpyInstance;

  const mockTenants: TenantDto[] = [
    { id: 't1', name: 'Clínica A', slug: 'clinica-a', role: 'owner' },
    { id: 't2', name: 'Clínica B', slug: 'clinica-b', role: 'operator' },
  ];

  function setup() {
    tenantService = {
      getMyTenants: jest.fn(),
      switchTenant: jest.fn(),
    };

    currentUserService = { invalidate: jest.fn() };

    TestBed.configureTestingModule({
      imports: [TenantSelectComponent],
      providers: [
        provideRouter([]),
        { provide: TenantService, useValue: tenantService },
        { provide: CurrentUserService, useValue: currentUserService },
      ],
    });

    navigateSpy = jest.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
  }

  it('renders list of tenants from getMyTenants()', fakeAsync(() => {
    setup();
    tenantService.getMyTenants.mockReturnValue(of(mockTenants));
    const fixture = TestBed.createComponent(TenantSelectComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const buttons = fixture.nativeElement.querySelectorAll('button');
    expect(buttons).toHaveLength(2);
    expect(buttons[0].textContent).toContain('Clínica A');
    expect(buttons[1].textContent).toContain('Clínica B');
  }));

  it('calls switchTenant and navigates to / on tenant selection', fakeAsync(() => {
    setup();
    tenantService.getMyTenants.mockReturnValue(of(mockTenants));
    tenantService.switchTenant.mockReturnValue(of(null));
    const fixture = TestBed.createComponent(TenantSelectComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    fixture.componentInstance.selectTenant('t1');
    tick();

    expect(tenantService.switchTenant).toHaveBeenCalledWith('t1');
    expect(navigateSpy).toHaveBeenCalledWith(['/']);
  }));

  it('invalidates current user cache after successful switchTenant', fakeAsync(() => {
    setup();
    tenantService.getMyTenants.mockReturnValue(of(mockTenants));
    tenantService.switchTenant.mockReturnValue(of(null));
    const fixture = TestBed.createComponent(TenantSelectComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    fixture.componentInstance.selectTenant('t1');
    tick();

    expect(currentUserService.invalidate).toHaveBeenCalled();
    expect(navigateSpy).toHaveBeenCalledWith(['/']);
  }));

  it('shows error when switchTenant fails', fakeAsync(() => {
    setup();
    tenantService.getMyTenants.mockReturnValue(of(mockTenants));
    tenantService.switchTenant.mockReturnValue(throwError(() => new Error('fail')));
    const fixture = TestBed.createComponent(TenantSelectComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    fixture.componentInstance.selectTenant('t1');
    tick();
    fixture.detectChanges();

    expect(fixture.componentInstance.errorMessage()).toBeTruthy();
    expect(navigateSpy).not.toHaveBeenCalled();
  }));
});
