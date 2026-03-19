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

  it('should GET /api/procedures with credentials', () => {
    service.getProcedures().subscribe();
    const req = httpTesting.expectOne('/api/procedures');
    expect(req.request.method).toBe('GET');
    expect(req.request.withCredentials).toBe(true);
    req.flush([]);
  });

  it('should POST /api/procedures with command and credentials', () => {
    const command: CreateProcedureCommand = {
      code: '10101012',
      description: 'Consulta em Clínica Médica',
      value: 150.0,
    };

    service.createProcedure(command).subscribe();

    const req = httpTesting.expectOne('/api/procedures');
    expect(req.request.method).toBe('POST');
    expect(req.request.withCredentials).toBe(true);
    expect(req.request.body).toEqual(command);
    req.flush({ id: 'proc-1', ...command });
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
    req.flush({ id: 'proc-1', ...command });
  });
});
