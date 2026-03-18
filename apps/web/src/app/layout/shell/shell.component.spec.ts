import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ShellComponent } from './shell.component';
import { AuthService } from '../../auth/data-access/auth.service';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';

describe('ShellComponent', () => {
  const authService: Partial<AuthService> = { logout: jest.fn() };

  function setup() {
    TestBed.configureTestingModule({
      imports: [ShellComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: authService },
      ],
    });
  }

  it('renders router-outlet', () => {
    setup();
    const fixture = TestBed.createComponent(ShellComponent);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('router-outlet')).toBeTruthy();
  });

  it('starts with collapsed = false', () => {
    setup();
    const fixture = TestBed.createComponent(ShellComponent);
    fixture.detectChanges();
    expect(fixture.componentInstance.collapsed()).toBe(false);
  });

  it('toggles collapsed when sidebar emits toggleCollapse', fakeAsync(() => {
    setup();
    const fixture = TestBed.createComponent(ShellComponent);
    fixture.detectChanges();

    const sidebar = fixture.nativeElement.querySelector('app-sidebar');
    expect(sidebar).toBeTruthy();

    fixture.componentInstance.toggle();
    tick();
    fixture.detectChanges();

    expect(fixture.componentInstance.collapsed()).toBe(true);
  }));

  it('adds shell--collapsed class when collapsed is true', fakeAsync(() => {
    setup();
    const fixture = TestBed.createComponent(ShellComponent);
    fixture.detectChanges();

    fixture.componentInstance.collapsed.set(true);
    tick();
    fixture.detectChanges();

    const shell = fixture.nativeElement.querySelector('.shell');
    expect(shell.classList).toContain('shell--collapsed');
  }));
});
