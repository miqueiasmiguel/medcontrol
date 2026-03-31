import { signal } from '@angular/core';
import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';
import { SidebarComponent } from './sidebar.component';
import { AuthService } from '../../auth/data-access/auth.service';
import { CurrentUserService } from '../../core/data-access/current-user.service';
import { UserDto } from '../../settings/data-access/settings.service';

describe('SidebarComponent', () => {
  let authService: jest.Mocked<Pick<AuthService, 'logout'>>;
  let navigateSpy: jest.SpyInstance;

  function makeUser(partial: Partial<UserDto> = {}): UserDto {
    return {
      id: '1',
      email: 'user@example.com',
      displayName: 'João Silva',
      avatarUrl: null,
      isEmailVerified: true,
      globalRole: 'None',
      lastLoginAt: null,
      tenantRole: 'operator',
      tenantName: 'Clínica Teste',
      ...partial,
    };
  }

  function setup(collapsed = false, user: Partial<UserDto> = {}) {
    authService = { logout: jest.fn() };
    const userSignal = signal<UserDto | null>(makeUser(user));

    TestBed.configureTestingModule({
      imports: [SidebarComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authService },
        {
          provide: CurrentUserService,
          useValue: {
            getMe: jest.fn().mockReturnValue(of(makeUser(user))),
            isDoctor: signal(user.tenantRole === 'doctor'),
            currentUser: userSignal.asReadonly(),
            tenantName: signal(makeUser(user).tenantName ?? null),
          },
        },
      ],
    });

    navigateSpy = jest
      .spyOn(TestBed.inject(Router), 'navigate')
      .mockResolvedValue(true);

    const fixture = TestBed.createComponent(SidebarComponent);
    fixture.componentInstance.collapsed = collapsed;
    fixture.detectChanges();
    return fixture;
  }

  it('renders nav links', () => {
    const fixture = setup();
    const links = fixture.nativeElement.querySelectorAll('a.sidebar__item');
    expect(links.length).toBeGreaterThanOrEqual(1);
  });

  it('shows brand text when not collapsed', () => {
    const fixture = setup(false);
    expect(fixture.nativeElement.querySelector('.sidebar__brand')).toBeTruthy();
  });

  it('hides brand text when collapsed', () => {
    const fixture = setup(true);
    expect(fixture.nativeElement.querySelector('.sidebar__brand')).toBeFalsy();
  });

  it('applies sidebar--collapsed class when collapsed=true', () => {
    const fixture = setup(true);
    const nav = fixture.nativeElement.querySelector('.sidebar');
    expect(nav.classList).toContain('sidebar--collapsed');
  });

  it('emits toggleCollapse when toggle button clicked', () => {
    const fixture = setup();
    const emitSpy = jest.spyOn(fixture.componentInstance.toggleCollapse, 'emit');
    const btn = fixture.nativeElement.querySelector('.sidebar__toggle');
    btn.click();
    expect(emitSpy).toHaveBeenCalled();
  });

  it('calls logout and navigates to /auth/login on logout click', fakeAsync(() => {
    const fixture = setup();
    authService.logout.mockReturnValue(of(null));

    fixture.componentInstance.logout();
    tick();

    expect(authService.logout).toHaveBeenCalled();
    expect(navigateSpy).toHaveBeenCalledWith(['/auth/login']);
  }));

  it('shows user display name in user card', () => {
    const fixture = setup(false, { displayName: 'Maria Souza' });
    const name = fixture.nativeElement.querySelector('.sidebar__user-name');
    expect(name?.textContent?.trim()).toBe('Maria Souza');
  });

  it('shows email when displayName is null', () => {
    const fixture = setup(false, { displayName: null, email: 'maria@example.com' });
    const name = fixture.nativeElement.querySelector('.sidebar__user-name');
    expect(name?.textContent?.trim()).toBe('maria@example.com');
  });

  it('shows tenant name in user card', () => {
    const fixture = setup(false, { tenantName: 'Hospital Central' });
    const tenant = fixture.nativeElement.querySelector('.sidebar__user-tenant');
    expect(tenant?.textContent?.trim()).toBe('Hospital Central');
  });

  it('shows role label in user card', () => {
    const fixture = setup(false, { tenantRole: 'doctor' });
    const role = fixture.nativeElement.querySelector('.sidebar__user-role');
    expect(role?.textContent?.trim()).toBe('Médico');
  });

  it('shows only avatar when collapsed', () => {
    const fixture = setup(true);
    expect(fixture.nativeElement.querySelector('.sidebar__user-info')).toBeFalsy();
    expect(fixture.nativeElement.querySelector('.sidebar__user-avatar')).toBeTruthy();
  });

  it('computes correct initials from display name', () => {
    const fixture = setup(false, { displayName: 'Ana Paula Costa' });
    const avatar = fixture.nativeElement.querySelector('.sidebar__user-avatar');
    expect(avatar?.textContent?.trim()).toBe('AC');
  });
});
