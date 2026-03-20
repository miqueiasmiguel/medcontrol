import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { MembersComponent } from './members.component';
import { MembersService, MemberDto } from './data-access/members.service';

describe('MembersComponent', () => {
  let membersService: jest.Mocked<Pick<MembersService, 'getMembers' | 'removeMember'>>;

  const mockMembers: MemberDto[] = [
    {
      userId: 'u-1',
      displayName: 'João Silva',
      email: 'joao@example.com',
      avatarUrl: null,
      role: 'admin',
      joinedAt: '2024-01-15T00:00:00Z',
    },
    {
      userId: 'u-2',
      displayName: 'Maria Costa',
      email: 'maria@example.com',
      avatarUrl: null,
      role: 'operator',
      joinedAt: '2024-02-20T00:00:00Z',
    },
  ];

  function setup() {
    membersService = {
      getMembers: jest.fn(),
      removeMember: jest.fn(),
    };

    TestBed.configureTestingModule({
      imports: [MembersComponent],
      providers: [
        provideRouter([]),
        { provide: MembersService, useValue: membersService },
      ],
    });
  }

  it('renders list of members on init', fakeAsync(() => {
    setup();
    membersService.getMembers.mockReturnValue(of(mockMembers));
    const fixture = TestBed.createComponent(MembersComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const rows = fixture.nativeElement.querySelectorAll('.members__row');
    expect(rows).toHaveLength(2);
    expect(rows[0].textContent).toContain('João Silva');
    expect(rows[1].textContent).toContain('Maria Costa');
  }));

  it('shows empty state when no members', fakeAsync(() => {
    setup();
    membersService.getMembers.mockReturnValue(of([]));
    const fixture = TestBed.createComponent(MembersComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.members__empty')).toBeTruthy();
  }));

  it('shows error message on load failure', fakeAsync(() => {
    setup();
    membersService.getMembers.mockReturnValue(throwError(() => new Error('fail')));
    const fixture = TestBed.createComponent(MembersComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    expect(fixture.componentInstance.errorMessage()).toBeTruthy();
  }));

  it('opens form in add mode when "Adicionar membro" clicked', fakeAsync(() => {
    setup();
    membersService.getMembers.mockReturnValue(of([]));
    const fixture = TestBed.createComponent(MembersComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const btn = fixture.nativeElement.querySelector('.members__header .btn--primary');
    btn.click();
    fixture.detectChanges();

    expect(fixture.componentInstance.formOpen()).toBe(true);
    expect(fixture.componentInstance.selectedMember()).toBeNull();
  }));

  it('opens form in edit mode when edit button clicked', fakeAsync(() => {
    setup();
    membersService.getMembers.mockReturnValue(of(mockMembers));
    const fixture = TestBed.createComponent(MembersComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const editBtns = fixture.nativeElement.querySelectorAll('.btn--ghost:not(.btn--danger)');
    editBtns[0].click();
    fixture.detectChanges();

    expect(fixture.componentInstance.formOpen()).toBe(true);
    expect(fixture.componentInstance.selectedMember()?.userId).toBe('u-1');
  }));

  it('closes form and resets selectedMember on closeForm', fakeAsync(() => {
    setup();
    membersService.getMembers.mockReturnValue(of(mockMembers));
    const fixture = TestBed.createComponent(MembersComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    fixture.componentInstance.openEditForm(mockMembers[0]);
    fixture.detectChanges();
    expect(fixture.componentInstance.formOpen()).toBe(true);

    fixture.componentInstance.closeForm();
    fixture.detectChanges();
    expect(fixture.componentInstance.formOpen()).toBe(false);
    expect(fixture.componentInstance.selectedMember()).toBeNull();
  }));

  it('adds new member to list on onSaved with new member', fakeAsync(() => {
    setup();
    membersService.getMembers.mockReturnValue(of(mockMembers));
    const fixture = TestBed.createComponent(MembersComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const newMember: MemberDto = {
      userId: 'u-3',
      displayName: 'Carlos Lima',
      email: 'carlos@example.com',
      avatarUrl: null,
      role: 'doctor',
      joinedAt: new Date().toISOString(),
    };

    fixture.componentInstance.onSaved(newMember);
    expect(fixture.componentInstance.members()).toHaveLength(3);
    expect(fixture.componentInstance.members()[2].userId).toBe('u-3');
  }));

  it('updates existing member in list on onSaved with existing id', fakeAsync(() => {
    setup();
    membersService.getMembers.mockReturnValue(of(mockMembers));
    const fixture = TestBed.createComponent(MembersComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const updated: MemberDto = { ...mockMembers[0], role: 'doctor' };
    fixture.componentInstance.onSaved(updated);

    expect(fixture.componentInstance.members()).toHaveLength(2);
    expect(fixture.componentInstance.members()[0].role).toBe('doctor');
  }));

  it('removes member from list on removeMember success', fakeAsync(() => {
    setup();
    membersService.getMembers.mockReturnValue(of(mockMembers));
    membersService.removeMember.mockReturnValue(of(undefined));
    const fixture = TestBed.createComponent(MembersComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    fixture.componentInstance.removeMember(mockMembers[0]);
    tick();
    fixture.detectChanges();

    expect(fixture.componentInstance.members()).toHaveLength(1);
    expect(fixture.componentInstance.members()[0].userId).toBe('u-2');
  }));

  it('getRoleLabel returns correct label for known roles', fakeAsync(() => {
    setup();
    membersService.getMembers.mockReturnValue(of([]));
    const fixture = TestBed.createComponent(MembersComponent);
    fixture.detectChanges();
    tick();

    expect(fixture.componentInstance.getRoleLabel('admin')).toBe('Admin');
    expect(fixture.componentInstance.getRoleLabel('operator')).toBe('Operador');
    expect(fixture.componentInstance.getRoleLabel('doctor')).toBe('Médico');
    expect(fixture.componentInstance.getRoleLabel('owner')).toBe('Proprietário');
  }));
});
