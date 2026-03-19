import { signal } from '@angular/core';
import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { SettingsComponent } from './settings.component';
import { SettingsService, UserDto } from './data-access/settings.service';
import { ThemeService } from './data-access/theme.service';

const mockUser: UserDto = {
  id: 'u1',
  email: 'user@example.com',
  displayName: 'João Silva',
  avatarUrl: null,
  isEmailVerified: true,
  globalRole: 'None',
  lastLoginAt: null,
};

describe('SettingsComponent', () => {
  let settingsService: jest.Mocked<Pick<SettingsService, 'getMe' | 'updateProfile'>>;
  let themeService: Pick<ThemeService, 'apply' | 'theme'>;

  function setup() {
    settingsService = {
      getMe: jest.fn(),
      updateProfile: jest.fn(),
    };

    themeService = {
      apply: jest.fn(),
      theme: signal('system'),
    };

    TestBed.configureTestingModule({
      imports: [SettingsComponent, ReactiveFormsModule],
      providers: [
        provideNoopAnimations(),
        { provide: SettingsService, useValue: settingsService },
        { provide: ThemeService, useValue: themeService },
      ],
    });
  }

  it('loads profile on init and populates form', fakeAsync(() => {
    setup();
    settingsService.getMe.mockReturnValue(of(mockUser));
    const fixture = TestBed.createComponent(SettingsComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    expect(settingsService.getMe).toHaveBeenCalled();
    expect(fixture.componentInstance.profileForm.value.displayName).toBe('João Silva');
    expect(fixture.componentInstance.loading()).toBe(false);
  }));

  it('shows error message when profile load fails', fakeAsync(() => {
    setup();
    settingsService.getMe.mockReturnValue(throwError(() => new Error('fail')));
    const fixture = TestBed.createComponent(SettingsComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    expect(fixture.componentInstance.errorMessage()).toBeTruthy();
  }));

  it('calls updateProfile and shows success on save', fakeAsync(() => {
    setup();
    settingsService.getMe.mockReturnValue(of(mockUser));
    settingsService.updateProfile.mockReturnValue(of({ ...mockUser, displayName: 'Novo Nome' }));
    const fixture = TestBed.createComponent(SettingsComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    fixture.componentInstance.profileForm.setValue({ displayName: 'Novo Nome' });
    fixture.componentInstance.saveProfile();
    tick();
    fixture.detectChanges();

    expect(settingsService.updateProfile).toHaveBeenCalledWith({ displayName: 'Novo Nome' });
    expect(fixture.componentInstance.successMessage()).toBeTruthy();
  }));

  it('shows error message when save fails', fakeAsync(() => {
    setup();
    settingsService.getMe.mockReturnValue(of(mockUser));
    settingsService.updateProfile.mockReturnValue(throwError(() => new Error('fail')));
    const fixture = TestBed.createComponent(SettingsComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    fixture.componentInstance.profileForm.setValue({ displayName: 'Novo Nome' });
    fixture.componentInstance.saveProfile();
    tick();
    fixture.detectChanges();

    expect(fixture.componentInstance.errorMessage()).toBeTruthy();
    expect(fixture.componentInstance.saving()).toBe(false);
  }));

  it('calls themeService.apply when applyTheme is called', fakeAsync(() => {
    setup();
    settingsService.getMe.mockReturnValue(of(mockUser));
    const fixture = TestBed.createComponent(SettingsComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    fixture.componentInstance.applyTheme('dark');

    expect(themeService.apply).toHaveBeenCalledWith('dark');
  }));

  it('does not call updateProfile when form is invalid', fakeAsync(() => {
    setup();
    settingsService.getMe.mockReturnValue(of(mockUser));
    const fixture = TestBed.createComponent(SettingsComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    fixture.componentInstance.profileForm.setValue({ displayName: 'A'.repeat(101) });
    fixture.componentInstance.saveProfile();
    tick();

    expect(settingsService.updateProfile).not.toHaveBeenCalled();
  }));
});
