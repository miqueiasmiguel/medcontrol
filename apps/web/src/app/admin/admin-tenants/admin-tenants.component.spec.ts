import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { of } from 'rxjs';
import { AdminTenantsComponent } from './admin-tenants.component';
import { AdminTenantsService, AdminTenantDto } from '../data-access/admin-tenants.service';

const tenant1: AdminTenantDto = {
  id: 'tenant-1',
  name: 'Clinic A',
  slug: 'clinic-a',
  isActive: true,
  createdAt: '2024-01-15T10:00:00Z',
  memberCount: 5,
};

const tenant2: AdminTenantDto = {
  id: 'tenant-2',
  name: 'Clinic B',
  slug: 'clinic-b',
  isActive: false,
  createdAt: '2024-02-01T10:00:00Z',
  memberCount: 2,
};

describe('AdminTenantsComponent', () => {
  let service: { listTenants: jest.Mock; setTenantStatus: jest.Mock };

  beforeEach(() => {
    service = {
      listTenants: jest.fn().mockReturnValue(of([tenant1, tenant2])),
      setTenantStatus: jest.fn().mockReturnValue(of(null)),
    };

    TestBed.configureTestingModule({
      imports: [AdminTenantsComponent],
      providers: [{ provide: AdminTenantsService, useValue: service }],
    });
  });

  it('renders tenant names after load', fakeAsync(() => {
    const fixture = TestBed.createComponent(AdminTenantsComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Clinic A');
    expect(text).toContain('Clinic B');
  }));

  it('shows member count', fakeAsync(() => {
    const fixture = TestBed.createComponent(AdminTenantsComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('5');
    expect(text).toContain('2');
  }));

  it('calls setTenantStatus when toggle is clicked', fakeAsync(() => {
    const fixture = TestBed.createComponent(AdminTenantsComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const buttons = fixture.debugElement.queryAll(By.css('[data-testid="toggle-status"]'));
    expect(buttons.length).toBeGreaterThan(0);
    buttons[0].nativeElement.click();
    tick();

    expect(service.setTenantStatus).toHaveBeenCalled();
  }));
});
