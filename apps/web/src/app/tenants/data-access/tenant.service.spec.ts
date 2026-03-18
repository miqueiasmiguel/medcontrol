import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TenantService } from './tenant.service';

describe('TenantService', () => {
  let service: TenantService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [TenantService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(TenantService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('should GET /api/tenants/me with credentials', () => {
    service.getMyTenants().subscribe();
    const req = httpTesting.expectOne('/api/tenants/me');
    expect(req.request.method).toBe('GET');
    expect(req.request.withCredentials).toBe(true);
    req.flush([]);
  });

  it('should POST /api/tenants with name and credentials', () => {
    service.createTenant('Clínica Saúde').subscribe();
    const req = httpTesting.expectOne('/api/tenants');
    expect(req.request.method).toBe('POST');
    expect(req.request.withCredentials).toBe(true);
    expect(req.request.body).toEqual({ name: 'Clínica Saúde' });
    req.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('should POST /api/tenants/switch with tenantId and credentials', () => {
    service.switchTenant('tenant-id-123').subscribe();
    const req = httpTesting.expectOne('/api/tenants/switch');
    expect(req.request.method).toBe('POST');
    expect(req.request.withCredentials).toBe(true);
    expect(req.request.body).toEqual({ tenantId: 'tenant-id-123' });
    req.flush(null, { status: 204, statusText: 'No Content' });
  });
});
