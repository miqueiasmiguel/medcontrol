import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { MemberFormComponent } from './member-form.component';
import { MembersService, MemberDto } from '../data-access/members.service';

describe('MemberFormComponent', () => {
  let membersService: jest.Mocked<Pick<MembersService, 'addMember' | 'updateMemberRole'>>;

  const mockMember: MemberDto = {
    userId: 'u-1',
    displayName: 'João Silva',
    email: 'joao@example.com',
    avatarUrl: null,
    role: 'operator',
    joinedAt: new Date().toISOString(),
    invited: false,
  };

  function setup() {
    membersService = {
      addMember: jest.fn(),
      updateMemberRole: jest.fn(),
    };

    TestBed.configureTestingModule({
      imports: [MemberFormComponent],
      providers: [
        provideRouter([]),
        { provide: MembersService, useValue: membersService },
      ],
    });
  }

  it('renders in create mode when member input is null', () => {
    setup();
    const fixture = TestBed.createComponent(MemberFormComponent);
    fixture.componentRef.setInput('member', null);
    fixture.detectChanges();

    expect(fixture.componentInstance.isEditing).toBe(false);
    expect(fixture.nativeElement.querySelector('[type="submit"]').textContent).toContain(
      'Adicionar membro',
    );
  });

  it('renders in edit mode when member input is provided', () => {
    setup();
    const fixture = TestBed.createComponent(MemberFormComponent);
    fixture.componentRef.setInput('member', mockMember);
    fixture.detectChanges();

    expect(fixture.componentInstance.isEditing).toBe(true);
    expect(fixture.nativeElement.querySelector('[type="submit"]').textContent).toContain(
      'Salvar alterações',
    );
  });

  it('patches role from member input in edit mode', () => {
    setup();
    const fixture = TestBed.createComponent(MemberFormComponent);
    fixture.componentRef.setInput('member', mockMember);
    fixture.detectChanges();

    expect(fixture.componentInstance.form.controls.role.value).toBe('operator');
  });

  it('marks all fields as touched and does not submit when form is invalid', () => {
    setup();
    const fixture = TestBed.createComponent(MemberFormComponent);
    fixture.componentRef.setInput('member', null);
    fixture.detectChanges();

    fixture.componentInstance.submit();

    expect(fixture.componentInstance.form.touched).toBe(true);
    expect(membersService.addMember).not.toHaveBeenCalled();
  });

  it('calls addMember and emits saved on success', fakeAsync(() => {
    setup();
    membersService.addMember.mockReturnValue(of(mockMember));

    const fixture = TestBed.createComponent(MemberFormComponent);
    fixture.componentRef.setInput('member', null);
    fixture.detectChanges();

    fixture.componentInstance.form.setValue({ email: 'joao@example.com', role: 'operator' });

    const savedSpy = jest.spyOn(fixture.componentInstance.saved, 'emit');
    fixture.componentInstance.submit();
    tick();

    expect(membersService.addMember).toHaveBeenCalledWith({
      email: 'joao@example.com',
      role: 'operator',
    });
    expect(savedSpy).toHaveBeenCalledWith(mockMember);
    expect(fixture.componentInstance.loading()).toBe(false);
  }));

  it('calls updateMemberRole when editing', fakeAsync(() => {
    setup();
    const updated = { ...mockMember, role: 'doctor' };
    membersService.updateMemberRole.mockReturnValue(of(updated));

    const fixture = TestBed.createComponent(MemberFormComponent);
    fixture.componentRef.setInput('member', mockMember);
    fixture.detectChanges();

    fixture.componentInstance.form.patchValue({ role: 'doctor' });

    const savedSpy = jest.spyOn(fixture.componentInstance.saved, 'emit');
    fixture.componentInstance.submit();
    tick();

    expect(membersService.updateMemberRole).toHaveBeenCalledWith('u-1', { role: 'doctor' });
    expect(savedSpy).toHaveBeenCalledWith(updated);
  }));

  it('shows conflict error on 409 response', fakeAsync(() => {
    setup();
    const error = new HttpErrorResponse({ status: 409 });
    membersService.addMember.mockReturnValue(throwError(() => error));

    const fixture = TestBed.createComponent(MemberFormComponent);
    fixture.componentRef.setInput('member', null);
    fixture.detectChanges();

    fixture.componentInstance.form.setValue({ email: 'joao@example.com', role: 'operator' });
    fixture.componentInstance.submit();
    tick();
    fixture.detectChanges();

    expect(fixture.componentInstance.errorMessage()).toContain('membro');
    expect(fixture.componentInstance.loading()).toBe(false);
  }));

  it('shows invite banner and delays saved emit when invited is true', fakeAsync(() => {
    setup();
    const invitedMember: MemberDto = { ...mockMember, invited: true };
    membersService.addMember.mockReturnValue(of(invitedMember));

    const fixture = TestBed.createComponent(MemberFormComponent);
    fixture.componentRef.setInput('member', null);
    fixture.detectChanges();

    fixture.componentInstance.form.setValue({ email: 'new@example.com', role: 'operator' });

    const savedSpy = jest.spyOn(fixture.componentInstance.saved, 'emit');
    fixture.componentInstance.submit();
    tick(0);
    fixture.detectChanges();

    expect(fixture.componentInstance.invitedMessage()).toBeTruthy();
    expect(savedSpy).not.toHaveBeenCalled();

    tick(2500);
    fixture.detectChanges();

    expect(savedSpy).toHaveBeenCalledWith(invitedMember);
    expect(fixture.componentInstance.invitedMessage()).toBe('');
  }));

  it('shows generic error on non-409 failure', fakeAsync(() => {
    setup();
    const error = new HttpErrorResponse({ status: 500 });
    membersService.addMember.mockReturnValue(throwError(() => error));

    const fixture = TestBed.createComponent(MemberFormComponent);
    fixture.componentRef.setInput('member', null);
    fixture.detectChanges();

    fixture.componentInstance.form.setValue({ email: 'joao@example.com', role: 'operator' });
    fixture.componentInstance.submit();
    tick();
    fixture.detectChanges();

    expect(fixture.componentInstance.errorMessage()).toBeTruthy();
    expect(fixture.componentInstance.loading()).toBe(false);
  }));

  it('emits closed when close() is called', () => {
    setup();
    const fixture = TestBed.createComponent(MemberFormComponent);
    fixture.componentRef.setInput('member', null);
    fixture.detectChanges();

    const closedSpy = jest.spyOn(fixture.componentInstance.closed, 'emit');
    fixture.componentInstance.close();

    expect(closedSpy).toHaveBeenCalled();
  });
});
