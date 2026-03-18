import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { TenantNewComponent } from './tenant-new.component';
import { TenantService } from '../data-access/tenant.service';

describe('TenantNewComponent', () => {
  let tenantService: jest.Mocked<Pick<TenantService, 'createTenant'>>;
  let navigateSpy: jest.SpyInstance;

  function setup() {
    tenantService = {
      createTenant: jest.fn(),
    };

    TestBed.configureTestingModule({
      imports: [TenantNewComponent],
      providers: [
        provideRouter([]),
        { provide: TenantService, useValue: tenantService },
      ],
    });

    navigateSpy = jest.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
  }

  it('renders form with name field', () => {
    setup();
    const fixture = TestBed.createComponent(TenantNewComponent);
    fixture.detectChanges();
    const input = fixture.nativeElement.querySelector('input');
    expect(input).toBeTruthy();
  });

  it('calls createTenant on submit and navigates to / on success', fakeAsync(() => {
    setup();
    tenantService.createTenant.mockReturnValue(of(null));
    const fixture = TestBed.createComponent(TenantNewComponent);
    fixture.detectChanges();

    const component = fixture.componentInstance;
    component.form.setValue({ name: 'Minha Clínica' });
    component.onSubmit();
    tick();

    expect(tenantService.createTenant).toHaveBeenCalledWith('Minha Clínica');
    expect(navigateSpy).toHaveBeenCalledWith(['/']);
  }));

  it('shows error message on failure', fakeAsync(() => {
    setup();
    tenantService.createTenant.mockReturnValue(throwError(() => new Error('fail')));
    const fixture = TestBed.createComponent(TenantNewComponent);
    fixture.detectChanges();

    const component = fixture.componentInstance;
    component.form.setValue({ name: 'Minha Clínica' });
    component.onSubmit();
    tick();
    fixture.detectChanges();

    expect(component.errorMessage()).toBeTruthy();
    expect(navigateSpy).not.toHaveBeenCalled();
  }));

  it('does not submit when form is invalid', fakeAsync(() => {
    setup();
    const fixture = TestBed.createComponent(TenantNewComponent);
    fixture.detectChanges();

    const component = fixture.componentInstance;
    component.form.setValue({ name: '' });
    component.onSubmit();
    tick();

    expect(tenantService.createTenant).not.toHaveBeenCalled();
  }));
});
