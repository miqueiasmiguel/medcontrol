import { signal } from '@angular/core';
import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';
import { SidebarComponent } from './sidebar.component';
import { AuthService } from '../../auth/data-access/auth.service';
import { CurrentUserService } from '../../core/data-access/current-user.service';

describe('SidebarComponent', () => {
  let authService: jest.Mocked<Pick<AuthService, 'logout'>>;
  let currentUserService: jest.Mocked<Pick<CurrentUserService, 'getMe'>>;
  let navigateSpy: jest.SpyInstance;

  function setup(collapsed = false) {
    authService = { logout: jest.fn() };
    currentUserService = { getMe: jest.fn().mockReturnValue(of({ tenantRole: 'operator' })) };

    TestBed.configureTestingModule({
      imports: [SidebarComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authService },
        { provide: CurrentUserService, useValue: { ...currentUserService, isDoctor: signal(false) } },
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
});
