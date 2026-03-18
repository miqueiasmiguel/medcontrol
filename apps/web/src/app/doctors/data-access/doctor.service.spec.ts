import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { DoctorService, CreateDoctorCommand } from './doctor.service';

describe('DoctorService', () => {
  let service: DoctorService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [DoctorService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(DoctorService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('should GET /api/doctors with credentials', () => {
    service.getDoctors().subscribe();
    const req = httpTesting.expectOne('/api/doctors');
    expect(req.request.method).toBe('GET');
    expect(req.request.withCredentials).toBe(true);
    req.flush([]);
  });

  it('should POST /api/doctors with command and credentials', () => {
    const command: CreateDoctorCommand = {
      name: 'Dr. João Silva',
      crm: '123456',
      councilState: 'SP',
      specialty: 'Cardiologia',
    };

    service.createDoctor(command).subscribe();

    const req = httpTesting.expectOne('/api/doctors');
    expect(req.request.method).toBe('POST');
    expect(req.request.withCredentials).toBe(true);
    expect(req.request.body).toEqual(command);
    req.flush({ id: 'doc-1', ...command });
  });

  it('should PATCH /api/doctors/:id with command and credentials', () => {
    const command = {
      name: 'Dr. João Silva',
      crm: '123456',
      councilState: 'SP',
      specialty: 'Cardiologia',
    };

    service.updateDoctor('doc-1', command).subscribe();

    const req = httpTesting.expectOne('/api/doctors/doc-1');
    expect(req.request.method).toBe('PATCH');
    expect(req.request.withCredentials).toBe(true);
    expect(req.request.body).toEqual(command);
    req.flush({ id: 'doc-1', ...command });
  });
});
