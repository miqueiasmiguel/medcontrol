import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AdminTenantsService, AdminTenantDto } from './admin-tenants.service';

const mockTenant: AdminTenantDto = {
  id: 'tenant-1',
  name: 'Clinic A',
  slug: 'clinic-a',
  isActive: true,
  createdAt: '2024-01-01T00:00:00Z',
  memberCount: 3,
};

describe('AdminTenantsService', () => {
  let service: AdminTenantsService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AdminTenantsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('listTenants() calls GET /api/admin/tenants', (done) => {
    service.listTenants().subscribe((tenants) => {
      expect(tenants).toEqual([mockTenant]);
      done();
    });

    const req = httpMock.expectOne('/api/admin/tenants');
    expect(req.request.method).toBe('GET');
    req.flush([mockTenant]);
  });

  it('setTenantStatus() calls PATCH /api/admin/tenants/{id}/status', (done) => {
    service.setTenantStatus('tenant-1', false).subscribe(() => done());

    const req = httpMock.expectOne('/api/admin/tenants/tenant-1/status');
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({ isActive: false });
    req.flush(null);
  });
});
