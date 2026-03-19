import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ProcedureService, CreateProcedureCommand } from './procedure.service';

describe('ProcedureService', () => {
  let service: ProcedureService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ProcedureService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ProcedureService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('should GET /api/procedures?activeOnly=true with credentials by default', () => {
    service.getProcedures().subscribe();
    const req = httpTesting.expectOne('/api/procedures?activeOnly=true');
    expect(req.request.method).toBe('GET');
    expect(req.request.withCredentials).toBe(true);
    req.flush([]);
  });

  it('should GET /api/procedures?activeOnly=false when activeOnly=false', () => {
    service.getProcedures(false).subscribe();
    const req = httpTesting.expectOne('/api/procedures?activeOnly=false');
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should POST /api/procedures with command and credentials', () => {
    const command: CreateProcedureCommand = {
      code: '10101012',
      description: 'Consulta em Clínica Médica',
      value: 150.0,
      effectiveFrom: '2025-01-01',
    };

    service.createProcedure(command).subscribe();

    const req = httpTesting.expectOne('/api/procedures');
    expect(req.request.method).toBe('POST');
    expect(req.request.withCredentials).toBe(true);
    expect(req.request.body).toEqual(command);
    req.flush({ id: 'proc-1', ...command, effectiveTo: null, source: 'Manual' });
  });

  it('should PATCH /api/procedures/:id with command and credentials', () => {
    const command = {
      code: '10101012',
      description: 'Consulta em Clínica Médica',
      value: 200.0,
    };

    service.updateProcedure('proc-1', command).subscribe();

    const req = httpTesting.expectOne('/api/procedures/proc-1');
    expect(req.request.method).toBe('PATCH');
    expect(req.request.withCredentials).toBe(true);
    expect(req.request.body).toEqual(command);
    req.flush({ id: 'proc-1', ...command, effectiveFrom: '2025-01-01', effectiveTo: null, source: 'Manual' });
  });

  it('should POST /api/procedures/import with multipart form data', () => {
    const file = new File(['cd_tuss;ds_termo\n'], 'test.csv', { type: 'text/csv' });
    service.importProcedures(file, 'Tuss', '2025-01-01').subscribe();

    const req = httpTesting.expectOne('/api/procedures/import');
    expect(req.request.method).toBe('POST');
    expect(req.request.withCredentials).toBe(true);
    expect(req.request.body).toBeInstanceOf(FormData);
    req.flush({
      id: 'import-1',
      source: 'Tuss',
      effectiveFrom: '2025-01-01',
      totalRows: 0,
      importedRows: 0,
      skippedRows: 0,
      errorSummary: null,
      createdAt: new Date().toISOString(),
    });
  });

  it('should GET /api/procedures/imports with credentials', () => {
    service.getImports().subscribe();
    const req = httpTesting.expectOne('/api/procedures/imports');
    expect(req.request.method).toBe('GET');
    expect(req.request.withCredentials).toBe(true);
    req.flush([]);
  });
});
