import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { DoctorFormComponent } from './doctor-form.component';
import { DoctorService, DoctorDto } from '../data-access/doctor.service';

describe('DoctorFormComponent', () => {
  let doctorService: jest.Mocked<Pick<DoctorService, 'createDoctor' | 'updateDoctor'>>;

  const mockDoctor: DoctorDto = {
    id: 'doc-1',
    tenantId: 'tenant-1',
    userId: null,
    name: 'Dr. João Silva',
    crm: '123456',
    councilState: 'SP',
    specialty: 'Cardiologia',
  };

  const validFormValue = {
    name: 'Dr. João Silva',
    crm: '123456',
    councilState: 'SP',
    specialty: 'Cardiologia',
    inviteCheckbox: false,
    inviteEmail: '',
  };

  function setup() {
    doctorService = {
      createDoctor: jest.fn(),
      updateDoctor: jest.fn(),
    };

    TestBed.configureTestingModule({
      imports: [DoctorFormComponent],
      providers: [
        provideRouter([]),
        { provide: DoctorService, useValue: doctorService },
      ],
    });
  }

  it('renders in create mode when doctor input is null', () => {
    setup();
    const fixture = TestBed.createComponent(DoctorFormComponent);
    fixture.componentRef.setInput('doctor', null);
    fixture.detectChanges();

    expect(fixture.componentInstance.isEditing).toBe(false);
    expect(fixture.nativeElement.querySelector('[type="submit"]').textContent).toContain(
      'Cadastrar médico',
    );
  });

  it('renders in edit mode when doctor input is provided', () => {
    setup();
    const fixture = TestBed.createComponent(DoctorFormComponent);
    fixture.componentRef.setInput('doctor', mockDoctor);
    fixture.detectChanges();

    expect(fixture.componentInstance.isEditing).toBe(true);
    expect(fixture.nativeElement.querySelector('[type="submit"]').textContent).toContain(
      'Salvar alterações',
    );
  });

  it('patches form values from doctor input', () => {
    setup();
    const fixture = TestBed.createComponent(DoctorFormComponent);
    fixture.componentRef.setInput('doctor', mockDoctor);
    fixture.detectChanges();

    const { form } = fixture.componentInstance;
    expect(form.value.name).toBe('Dr. João Silva');
    expect(form.value.crm).toBe('123456');
    expect(form.value.councilState).toBe('SP');
    expect(form.value.specialty).toBe('Cardiologia');
  });

  it('marks all fields as touched and does not submit when form is invalid', () => {
    setup();
    const fixture = TestBed.createComponent(DoctorFormComponent);
    fixture.componentRef.setInput('doctor', null);
    fixture.detectChanges();

    fixture.componentInstance.submit();

    expect(fixture.componentInstance.form.touched).toBe(true);
    expect(doctorService.createDoctor).not.toHaveBeenCalled();
  });

  it('calls createDoctor and emits saved on success', fakeAsync(() => {
    setup();
    doctorService.createDoctor.mockReturnValue(of(mockDoctor));

    const fixture = TestBed.createComponent(DoctorFormComponent);
    fixture.componentRef.setInput('doctor', null);
    fixture.detectChanges();

    fixture.componentInstance.form.setValue(validFormValue);

    const savedSpy = jest.spyOn(fixture.componentInstance.saved, 'emit');
    fixture.componentInstance.submit();
    tick();

    expect(doctorService.createDoctor).toHaveBeenCalledWith({
      name: 'Dr. João Silva',
      crm: '123456',
      councilState: 'SP',
      specialty: 'Cardiologia',
    });
    expect(savedSpy).toHaveBeenCalledWith(mockDoctor);
    expect(fixture.componentInstance.loading()).toBe(false);
  }));

  it('calls updateDoctor when editing', fakeAsync(() => {
    setup();
    const updated = { ...mockDoctor, specialty: 'Neurologia' };
    doctorService.updateDoctor.mockReturnValue(of(updated));

    const fixture = TestBed.createComponent(DoctorFormComponent);
    fixture.componentRef.setInput('doctor', mockDoctor);
    fixture.detectChanges();

    fixture.componentInstance.form.patchValue({ specialty: 'Neurologia' });

    const savedSpy = jest.spyOn(fixture.componentInstance.saved, 'emit');
    fixture.componentInstance.submit();
    tick();

    expect(doctorService.updateDoctor).toHaveBeenCalledWith(
      'doc-1',
      expect.objectContaining({ specialty: 'Neurologia' }),
    );
    expect(savedSpy).toHaveBeenCalledWith(updated);
  }));

  it('shows duplicate CRM error on 409 response', fakeAsync(() => {
    setup();
    const error = new HttpErrorResponse({ status: 409 });
    doctorService.createDoctor.mockReturnValue(throwError(() => error));

    const fixture = TestBed.createComponent(DoctorFormComponent);
    fixture.componentRef.setInput('doctor', null);
    fixture.detectChanges();

    fixture.componentInstance.form.setValue(validFormValue);
    fixture.componentInstance.submit();
    tick();
    fixture.detectChanges();

    expect(fixture.componentInstance.errorMessage()).toContain('CRM');
    expect(fixture.componentInstance.loading()).toBe(false);
  }));

  it('shows generic error on non-409 failure', fakeAsync(() => {
    setup();
    const error = new HttpErrorResponse({ status: 500 });
    doctorService.createDoctor.mockReturnValue(throwError(() => error));

    const fixture = TestBed.createComponent(DoctorFormComponent);
    fixture.componentRef.setInput('doctor', null);
    fixture.detectChanges();

    fixture.componentInstance.form.setValue(validFormValue);
    fixture.componentInstance.submit();
    tick();
    fixture.detectChanges();

    expect(fixture.componentInstance.errorMessage()).toBeTruthy();
    expect(fixture.componentInstance.loading()).toBe(false);
  }));

  it('emits closed when close() is called', () => {
    setup();
    const fixture = TestBed.createComponent(DoctorFormComponent);
    fixture.componentRef.setInput('doctor', null);
    fixture.detectChanges();

    const closedSpy = jest.spyOn(fixture.componentInstance.closed, 'emit');
    fixture.componentInstance.close();

    expect(closedSpy).toHaveBeenCalled();
  });

  it('validates CRM pattern — rejects non-numeric', () => {
    setup();
    const fixture = TestBed.createComponent(DoctorFormComponent);
    fixture.componentRef.setInput('doctor', null);
    fixture.detectChanges();

    const crmControl = fixture.componentInstance.form.controls.crm;
    crmControl.setValue('abc123');
    crmControl.markAsTouched();

    expect(crmControl.invalid).toBe(true);
    expect(crmControl.errors?.['pattern']).toBeTruthy();
  });

  it('validates councilState pattern — requires 2 uppercase letters', () => {
    setup();
    const fixture = TestBed.createComponent(DoctorFormComponent);
    fixture.componentRef.setInput('doctor', null);
    fixture.detectChanges();

    const ctrl = fixture.componentInstance.form.controls.councilState;
    ctrl.setValue('sp'); // lowercase
    expect(ctrl.invalid).toBe(true);

    ctrl.setValue('SPX'); // 3 chars
    expect(ctrl.invalid).toBe(true);

    ctrl.setValue('SP'); // valid
    expect(ctrl.valid).toBe(true);
  });

  // Invite checkbox tests

  it('shows invite checkbox only in create mode', () => {
    setup();
    const fixture = TestBed.createComponent(DoctorFormComponent);
    fixture.componentRef.setInput('doctor', null);
    fixture.detectChanges();

    const checkbox = fixture.nativeElement.querySelector('[formControlName="inviteCheckbox"]');
    expect(checkbox).not.toBeNull();
  });

  it('does not show invite checkbox in edit mode', () => {
    setup();
    const fixture = TestBed.createComponent(DoctorFormComponent);
    fixture.componentRef.setInput('doctor', mockDoctor);
    fixture.detectChanges();

    const checkbox = fixture.nativeElement.querySelector('[formControlName="inviteCheckbox"]');
    expect(checkbox).toBeNull();
  });

  it('calls createDoctor with inviteEmail when checkbox checked and email valid', fakeAsync(() => {
    setup();
    doctorService.createDoctor.mockReturnValue(of(mockDoctor));

    const fixture = TestBed.createComponent(DoctorFormComponent);
    fixture.componentRef.setInput('doctor', null);
    fixture.detectChanges();

    fixture.componentInstance.form.setValue({
      ...validFormValue,
      inviteCheckbox: true,
      inviteEmail: 'medico@clinica.com',
    });

    fixture.componentInstance.submit();
    tick();

    expect(doctorService.createDoctor).toHaveBeenCalledWith(
      expect.objectContaining({ inviteEmail: 'medico@clinica.com' }),
    );
  }));

  it('calls createDoctor without inviteEmail when checkbox unchecked', fakeAsync(() => {
    setup();
    doctorService.createDoctor.mockReturnValue(of(mockDoctor));

    const fixture = TestBed.createComponent(DoctorFormComponent);
    fixture.componentRef.setInput('doctor', null);
    fixture.detectChanges();

    fixture.componentInstance.form.setValue(validFormValue);
    fixture.componentInstance.submit();
    tick();

    const callArg = (doctorService.createDoctor as jest.Mock).mock.calls[0][0];
    expect(callArg.inviteEmail).toBeUndefined();
  }));

  it('emits createdWithoutInvite when created without invite', fakeAsync(() => {
    setup();
    doctorService.createDoctor.mockReturnValue(of(mockDoctor));

    const fixture = TestBed.createComponent(DoctorFormComponent);
    fixture.componentRef.setInput('doctor', null);
    fixture.detectChanges();

    const createdWithoutInviteSpy = jest.spyOn(
      fixture.componentInstance.createdWithoutInvite,
      'emit',
    );
    fixture.componentInstance.form.setValue(validFormValue);
    fixture.componentInstance.submit();
    tick();

    expect(createdWithoutInviteSpy).toHaveBeenCalledWith(mockDoctor);
  }));

  it('does not emit createdWithoutInvite when created with invite', fakeAsync(() => {
    setup();
    doctorService.createDoctor.mockReturnValue(of(mockDoctor));

    const fixture = TestBed.createComponent(DoctorFormComponent);
    fixture.componentRef.setInput('doctor', null);
    fixture.detectChanges();

    const createdWithoutInviteSpy = jest.spyOn(
      fixture.componentInstance.createdWithoutInvite,
      'emit',
    );
    fixture.componentInstance.form.setValue({
      ...validFormValue,
      inviteCheckbox: true,
      inviteEmail: 'medico@clinica.com',
    });
    fixture.componentInstance.submit();
    tick();

    expect(createdWithoutInviteSpy).not.toHaveBeenCalled();
  }));

  it('validates email field when checkbox is checked and email is invalid', fakeAsync(() => {
    setup();
    const fixture = TestBed.createComponent(DoctorFormComponent);
    fixture.componentRef.setInput('doctor', null);
    fixture.detectChanges();

    fixture.componentInstance.form.setValue({
      ...validFormValue,
      inviteCheckbox: true,
      inviteEmail: 'email-invalido',
    });

    fixture.componentInstance.submit();
    tick();

    expect(doctorService.createDoctor).not.toHaveBeenCalled();
    expect(fixture.componentInstance.form.controls.inviteEmail.invalid).toBe(true);
  }));
});
