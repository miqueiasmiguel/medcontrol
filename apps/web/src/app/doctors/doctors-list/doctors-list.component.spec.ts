import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { DoctorsListComponent } from './doctors-list.component';
import { DoctorService, DoctorDto } from '../data-access/doctor.service';

describe('DoctorsListComponent', () => {
  let doctorService: jest.Mocked<Pick<DoctorService, 'getDoctors'>>;

  const mockDoctors: DoctorDto[] = [
    {
      id: 'doc-1',
      name: 'Dr. João Silva',
      crm: '123456',
      councilState: 'SP',
      specialty: 'Cardiologia',
    },
    {
      id: 'doc-2',
      name: 'Dra. Maria Costa',
      crm: '654321',
      councilState: 'RJ',
      specialty: 'Pediatria',
    },
  ];

  function setup() {
    doctorService = { getDoctors: jest.fn() };

    TestBed.configureTestingModule({
      imports: [DoctorsListComponent],
      providers: [
        provideRouter([]),
        { provide: DoctorService, useValue: doctorService },
      ],
    });
  }

  it('renders list of doctors on init', fakeAsync(() => {
    setup();
    doctorService.getDoctors.mockReturnValue(of(mockDoctors));
    const fixture = TestBed.createComponent(DoctorsListComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const rows = fixture.nativeElement.querySelectorAll('.doctors-list__row');
    expect(rows).toHaveLength(2);
    expect(rows[0].textContent).toContain('Dr. João Silva');
    expect(rows[1].textContent).toContain('Dra. Maria Costa');
  }));

  it('shows empty state when no doctors', fakeAsync(() => {
    setup();
    doctorService.getDoctors.mockReturnValue(of([]));
    const fixture = TestBed.createComponent(DoctorsListComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.doctors-list__empty')).toBeTruthy();
  }));

  it('shows error message on load failure', fakeAsync(() => {
    setup();
    doctorService.getDoctors.mockReturnValue(throwError(() => new Error('fail')));
    const fixture = TestBed.createComponent(DoctorsListComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    expect(fixture.componentInstance.errorMessage()).toBeTruthy();
  }));

  it('opens form in create mode when "Novo médico" clicked', fakeAsync(() => {
    setup();
    doctorService.getDoctors.mockReturnValue(of([]));
    const fixture = TestBed.createComponent(DoctorsListComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const btn = fixture.nativeElement.querySelector('.doctors-list__header .btn--primary');
    btn.click();
    fixture.detectChanges();

    expect(fixture.componentInstance.formOpen()).toBe(true);
    expect(fixture.componentInstance.selectedDoctor()).toBeNull();
  }));

  it('opens form in edit mode when row clicked', fakeAsync(() => {
    setup();
    doctorService.getDoctors.mockReturnValue(of(mockDoctors));
    const fixture = TestBed.createComponent(DoctorsListComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const row = fixture.nativeElement.querySelector('.doctors-list__row');
    row.click();
    fixture.detectChanges();

    expect(fixture.componentInstance.formOpen()).toBe(true);
    expect(fixture.componentInstance.selectedDoctor()?.id).toBe('doc-1');
  }));

  it('closes form and resets selectedDoctor on closeForm', fakeAsync(() => {
    setup();
    doctorService.getDoctors.mockReturnValue(of(mockDoctors));
    const fixture = TestBed.createComponent(DoctorsListComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    fixture.componentInstance.openEditForm(mockDoctors[0]);
    fixture.detectChanges();
    expect(fixture.componentInstance.formOpen()).toBe(true);

    fixture.componentInstance.closeForm();
    fixture.detectChanges();
    expect(fixture.componentInstance.formOpen()).toBe(false);
    expect(fixture.componentInstance.selectedDoctor()).toBeNull();
  }));

  it('adds new doctor to list on onSaved with new doctor', fakeAsync(() => {
    setup();
    doctorService.getDoctors.mockReturnValue(of(mockDoctors));
    const fixture = TestBed.createComponent(DoctorsListComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const newDoctor: DoctorDto = {
      id: 'doc-3',
      name: 'Dr. Carlos Lima',
      crm: '999',
      councilState: 'MG',
      specialty: 'Ortopedia',
    };

    fixture.componentInstance.onSaved(newDoctor);
    expect(fixture.componentInstance.doctors()).toHaveLength(3);
    expect(fixture.componentInstance.doctors()[2].id).toBe('doc-3');
  }));

  it('updates existing doctor in list on onSaved with existing id', fakeAsync(() => {
    setup();
    doctorService.getDoctors.mockReturnValue(of(mockDoctors));
    const fixture = TestBed.createComponent(DoctorsListComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const updated: DoctorDto = { ...mockDoctors[0], specialty: 'Neurologia' };
    fixture.componentInstance.onSaved(updated);

    expect(fixture.componentInstance.doctors()).toHaveLength(2);
    expect(fixture.componentInstance.doctors()[0].specialty).toBe('Neurologia');
  }));
});
