import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { DoctorOnboardingComponent } from './onboarding.component';
import { DoctorService, DoctorDto } from '../data-access/doctor.service';
import { WINDOW } from '../../core/tokens/window.token';

describe('DoctorOnboardingComponent', () => {
  let doctorService: jest.Mocked<Pick<DoctorService, 'createMyDoctorProfile'>>;
  let mockWindow: { sessionStorage: Storage };

  const mockProfile: DoctorDto = {
    id: 'doc-1',
    tenantId: 'tenant-1',
    userId: 'user-1',
    name: 'Dr. João Silva',
    crm: '123456',
    councilState: 'SP',
    specialty: 'Cardiologia',
  };

  const validForm = {
    name: 'Dr. João Silva',
    crm: '123456',
    councilState: 'SP',
    specialty: 'Cardiologia',
  };

  function setup() {
    doctorService = { createMyDoctorProfile: jest.fn() };
    mockWindow = { sessionStorage: { getItem: jest.fn(), setItem: jest.fn(), removeItem: jest.fn(), clear: jest.fn(), key: jest.fn(), length: 0 } as unknown as Storage };

    TestBed.configureTestingModule({
      imports: [DoctorOnboardingComponent],
      providers: [
        provideRouter([{ path: 'doctors', children: [] }]),
        { provide: DoctorService, useValue: doctorService },
        { provide: WINDOW, useValue: mockWindow },
      ],
    });
  }

  it('renders all form fields', () => {
    setup();
    const fixture = TestBed.createComponent(DoctorOnboardingComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[formControlName="name"]')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('[formControlName="crm"]')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('[formControlName="councilState"]')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('[formControlName="specialty"]')).not.toBeNull();
  });

  it('does not submit when form is invalid', () => {
    setup();
    const fixture = TestBed.createComponent(DoctorOnboardingComponent);
    fixture.detectChanges();

    fixture.componentInstance.submit();

    expect(doctorService.createMyDoctorProfile).not.toHaveBeenCalled();
    expect(fixture.componentInstance.form.touched).toBe(true);
  });

  it('calls createMyDoctorProfile and navigates to /doctors on success', fakeAsync(() => {
    setup();
    doctorService.createMyDoctorProfile.mockReturnValue(of(mockProfile));

    const fixture = TestBed.createComponent(DoctorOnboardingComponent);
    fixture.detectChanges();
    fixture.componentInstance.form.setValue(validForm);

    const router = TestBed.inject(Router);
    const navSpy = jest.spyOn(router, 'navigate');

    fixture.componentInstance.submit();
    tick();

    expect(doctorService.createMyDoctorProfile).toHaveBeenCalledWith(validForm);
    expect(navSpy).toHaveBeenCalledWith(['/doctors']);
  }));

  it('shows error message on failure', fakeAsync(() => {
    setup();
    const error = new HttpErrorResponse({ status: 500 });
    doctorService.createMyDoctorProfile.mockReturnValue(throwError(() => error));

    const fixture = TestBed.createComponent(DoctorOnboardingComponent);
    fixture.detectChanges();
    fixture.componentInstance.form.setValue(validForm);

    fixture.componentInstance.submit();
    tick();
    fixture.detectChanges();

    expect(fixture.componentInstance.errorMessage()).toBeTruthy();
    expect(fixture.componentInstance.loading()).toBe(false);
  }));

  it('sets sessionStorage skip flag and navigates to /doctors when skipping', fakeAsync(() => {
    setup();
    const fixture = TestBed.createComponent(DoctorOnboardingComponent);
    fixture.detectChanges();

    const router = TestBed.inject(Router);
    const navSpy = jest.spyOn(router, 'navigate');

    fixture.componentInstance.skip();
    tick();

    expect(mockWindow.sessionStorage.setItem).toHaveBeenCalledWith('mmc_onboarding_skip', '1');
    expect(navSpy).toHaveBeenCalledWith(['/doctors']);
  }));
});
