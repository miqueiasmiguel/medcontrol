import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { DoctorLinkFormComponent } from './doctor-link-form.component';
import { DoctorService, DoctorDto } from '../data-access/doctor.service';
import { MembersService, MemberDto } from '../../members/data-access/members.service';

describe('DoctorLinkFormComponent', () => {
  let doctorService: jest.Mocked<Pick<DoctorService, 'getDoctors' | 'linkDoctorToUser' | 'inviteAndLinkMember'>>;
  let membersService: jest.Mocked<Pick<MembersService, 'getMembers'>>;

  const mockDoctor: DoctorDto = {
    id: 'doc-1',
    tenantId: 'tenant-1',
    userId: null,
    name: 'Dr. João Silva',
    crm: '123456',
    councilState: 'SP',
    specialty: 'Cardiologia',
  };

  const mockMember: MemberDto = {
    userId: 'user-1',
    email: 'joao@clinica.com',
    displayName: 'Dr. João Silva',
    role: 'doctor',
    invited: false,
  };

  function setup() {
    doctorService = {
      getDoctors: jest.fn(),
      linkDoctorToUser: jest.fn(),
      inviteAndLinkMember: jest.fn(),
    };
    membersService = {
      getMembers: jest.fn(),
    };

    doctorService.getDoctors.mockReturnValue(of([mockDoctor]));
    membersService.getMembers.mockReturnValue(of([mockMember]));

    TestBed.configureTestingModule({
      imports: [DoctorLinkFormComponent],
      providers: [
        provideRouter([]),
        { provide: DoctorService, useValue: doctorService },
        { provide: MembersService, useValue: membersService },
      ],
    });
  }

  it('renders existing member list on init', fakeAsync(() => {
    setup();
    const fixture = TestBed.createComponent(DoctorLinkFormComponent);
    fixture.componentRef.setInput('doctor', mockDoctor);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const options = fixture.nativeElement.querySelectorAll('.member-option');
    expect(options.length).toBe(1);
    expect(fixture.componentInstance.availableMembers().length).toBe(1);
  }));

  it('shows invite section when linkMode is invite', fakeAsync(() => {
    setup();
    const fixture = TestBed.createComponent(DoctorLinkFormComponent);
    fixture.componentRef.setInput('doctor', mockDoctor);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    fixture.componentInstance.linkMode.set('invite');
    fixture.detectChanges();

    const emailInput = fixture.nativeElement.querySelector('[formControlName="inviteEmail"]');
    expect(emailInput).not.toBeNull();
  }));

  it('hides invite section when linkMode is existing', fakeAsync(() => {
    setup();
    const fixture = TestBed.createComponent(DoctorLinkFormComponent);
    fixture.componentRef.setInput('doctor', mockDoctor);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    fixture.componentInstance.linkMode.set('existing');
    fixture.detectChanges();

    const emailInput = fixture.nativeElement.querySelector('[formControlName="inviteEmail"]');
    expect(emailInput).toBeNull();
  }));

  it('calls linkDoctorToUser when linkMode is existing and member selected', fakeAsync(() => {
    setup();
    const updated = { ...mockDoctor, userId: 'user-1' };
    doctorService.linkDoctorToUser.mockReturnValue(of(updated));

    const fixture = TestBed.createComponent(DoctorLinkFormComponent);
    fixture.componentRef.setInput('doctor', mockDoctor);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    fixture.componentInstance.form.controls.userId.setValue('user-1');

    const savedSpy = jest.spyOn(fixture.componentInstance.saved, 'emit');
    fixture.componentInstance.submit();
    tick();

    expect(doctorService.linkDoctorToUser).toHaveBeenCalledWith('doc-1', 'user-1');
    expect(savedSpy).toHaveBeenCalledWith(updated);
  }));

  it('calls inviteAndLinkMember when linkMode is invite and email is valid', fakeAsync(() => {
    setup();
    const updated = { ...mockDoctor, userId: 'user-2' };
    doctorService.inviteAndLinkMember.mockReturnValue(of(updated));

    const fixture = TestBed.createComponent(DoctorLinkFormComponent);
    fixture.componentRef.setInput('doctor', mockDoctor);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    fixture.componentInstance.linkMode.set('invite');
    fixture.detectChanges();
    fixture.componentInstance.form.controls.inviteEmail.setValue('novo@clinica.com');

    const savedSpy = jest.spyOn(fixture.componentInstance.saved, 'emit');
    fixture.componentInstance.submit();
    tick();

    expect(doctorService.inviteAndLinkMember).toHaveBeenCalledWith('doc-1', 'novo@clinica.com');
    expect(savedSpy).toHaveBeenCalledWith(updated);
  }));

  it('does not submit invite when email is invalid', fakeAsync(() => {
    setup();
    const fixture = TestBed.createComponent(DoctorLinkFormComponent);
    fixture.componentRef.setInput('doctor', mockDoctor);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    fixture.componentInstance.linkMode.set('invite');
    fixture.detectChanges();
    fixture.componentInstance.form.controls.inviteEmail.setValue('email-invalido');

    fixture.componentInstance.submit();
    tick();

    expect(doctorService.inviteAndLinkMember).not.toHaveBeenCalled();
    expect(fixture.componentInstance.form.controls.inviteEmail.invalid).toBe(true);
  }));

  it('shows error message on 409 in existing mode', fakeAsync(() => {
    setup();
    const error = new HttpErrorResponse({ status: 409 });
    doctorService.linkDoctorToUser.mockReturnValue(throwError(() => error));

    const fixture = TestBed.createComponent(DoctorLinkFormComponent);
    fixture.componentRef.setInput('doctor', mockDoctor);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    fixture.componentInstance.form.controls.userId.setValue('user-1');
    fixture.componentInstance.submit();
    tick();
    fixture.detectChanges();

    expect(fixture.componentInstance.errorMessage()).toContain('já está vinculado');
    expect(fixture.componentInstance.loading()).toBe(false);
  }));

  it('shows generic error on invite failure', fakeAsync(() => {
    setup();
    const error = new HttpErrorResponse({ status: 500 });
    doctorService.inviteAndLinkMember.mockReturnValue(throwError(() => error));

    const fixture = TestBed.createComponent(DoctorLinkFormComponent);
    fixture.componentRef.setInput('doctor', mockDoctor);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    fixture.componentInstance.linkMode.set('invite');
    fixture.detectChanges();
    fixture.componentInstance.form.controls.inviteEmail.setValue('novo@clinica.com');
    fixture.componentInstance.submit();
    tick();
    fixture.detectChanges();

    expect(fixture.componentInstance.errorMessage()).toBeTruthy();
    expect(fixture.componentInstance.loading()).toBe(false);
  }));

  it('shows invite option trigger button in empty state', fakeAsync(() => {
    setup();
    membersService.getMembers.mockReturnValue(of([]));
    doctorService.getDoctors.mockReturnValue(of([]));

    const fixture = TestBed.createComponent(DoctorLinkFormComponent);
    fixture.componentRef.setInput('doctor', mockDoctor);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const inviteBtn = fixture.nativeElement.querySelector('[data-testid="switch-to-invite"]');
    expect(inviteBtn).not.toBeNull();
  }));
});
