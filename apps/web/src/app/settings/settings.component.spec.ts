import { signal } from '@angular/core';
import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { provideRouter } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { SettingsComponent } from './settings.component';
import { SettingsService, UserDto } from './data-access/settings.service';
import { ThemeService } from './data-access/theme.service';
import { CurrentUserService } from '../core/data-access/current-user.service';
import { DoctorService, DoctorDto } from '../doctors/data-access/doctor.service';

const mockUser: UserDto = {
  id: 'u1',
  email: 'user@example.com',
  displayName: 'João Silva',
  avatarUrl: null,
  isEmailVerified: true,
  globalRole: 'None',
  lastLoginAt: null,
  tenantRole: 'operator',
};

const mockDoctorUser: UserDto = { ...mockUser, tenantRole: 'doctor' };

const mockDoctorProfile: DoctorDto = {
  id: 'd1',
  tenantId: 't1',
  userId: 'u1',
  name: 'Dr. João',
  crm: '123456',
  councilState: 'SP',
  specialty: 'Cardiologia',
};

describe('SettingsComponent', () => {
  let settingsService: jest.Mocked<Pick<SettingsService, 'updateProfile' | 'updateMyDoctorProfile'>>;
  let currentUserService: jest.Mocked<Pick<CurrentUserService, 'getMe'>>;
  let doctorService: jest.Mocked<Pick<DoctorService, 'getMyDoctorProfile'>>;
  let themeService: Pick<ThemeService, 'apply' | 'theme'>;

  function setup(isDoctor = false) {
    const user = isDoctor ? mockDoctorUser : mockUser;

    settingsService = {
      updateProfile: jest.fn(),
      updateMyDoctorProfile: jest.fn(),
    };

    currentUserService = {
      getMe: jest.fn().mockReturnValue(of(user)),
    };

    doctorService = {
      getMyDoctorProfile: jest.fn().mockReturnValue(of(mockDoctorProfile)),
    };

    themeService = {
      apply: jest.fn(),
      theme: signal('system'),
    };

    TestBed.configureTestingModule({
      imports: [SettingsComponent, ReactiveFormsModule],
      providers: [
        provideRouter([]),
        provideNoopAnimations(),
        { provide: SettingsService, useValue: settingsService },
        { provide: CurrentUserService, useValue: { ...currentUserService, isDoctor: signal(isDoctor) } },
        { provide: DoctorService, useValue: doctorService },
        { provide: ThemeService, useValue: themeService },
      ],
    });
  }

  it('loads profile on init and populates form', fakeAsync(() => {
    setup();
    const fixture = TestBed.createComponent(SettingsComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    expect(currentUserService.getMe).toHaveBeenCalled();
    expect(fixture.componentInstance.profileForm.value.displayName).toBe('João Silva');
    expect(fixture.componentInstance.loading()).toBe(false);
  }));

  it('shows error message when profile load fails', fakeAsync(() => {
    setup();
    currentUserService.getMe.mockReturnValue(throwError(() => new Error('fail')));
    const fixture = TestBed.createComponent(SettingsComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    expect(fixture.componentInstance.errorMessage()).toBeTruthy();
  }));

  it('calls updateProfile and shows success on save', fakeAsync(() => {
    setup();
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
    const fixture = TestBed.createComponent(SettingsComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    fixture.componentInstance.applyTheme('dark');

    expect(themeService.apply).toHaveBeenCalledWith('dark');
  }));

  it('does not call updateProfile when form is invalid', fakeAsync(() => {
    setup();
    const fixture = TestBed.createComponent(SettingsComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    fixture.componentInstance.profileForm.setValue({ displayName: 'A'.repeat(101) });
    fixture.componentInstance.saveProfile();
    tick();

    expect(settingsService.updateProfile).not.toHaveBeenCalled();
  }));

  it('does not render doctor profile card when user is not doctor', fakeAsync(() => {
    setup(false);
    const fixture = TestBed.createComponent(SettingsComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const el: HTMLElement = fixture.nativeElement;
    expect(el.textContent).not.toContain('Perfil médico');
    expect(doctorService.getMyDoctorProfile).not.toHaveBeenCalled();
  }));

  it('renders doctor profile card and loads data when user is doctor', fakeAsync(() => {
    setup(true);
    const fixture = TestBed.createComponent(SettingsComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    expect(doctorService.getMyDoctorProfile).toHaveBeenCalled();
    const el: HTMLElement = fixture.nativeElement;
    expect(el.textContent).toContain('Perfil médico');
    expect(fixture.componentInstance.doctorProfileForm.value.crm).toBe('123456');
    expect(fixture.componentInstance.doctorProfileForm.value.specialty).toBe('Cardiologia');
  }));

  it('calls updateMyDoctorProfile with correct values on saveDoctorProfile', fakeAsync(() => {
    setup(true);
    settingsService.updateMyDoctorProfile.mockReturnValue(of(null));
    const fixture = TestBed.createComponent(SettingsComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    fixture.componentInstance.doctorProfileForm.setValue({
      name: 'Dr. Carlos',
      crm: '654321',
      councilState: 'RJ',
      specialty: 'Neurologia',
    });
    fixture.componentInstance.saveDoctorProfile();
    tick();
    fixture.detectChanges();

    expect(settingsService.updateMyDoctorProfile).toHaveBeenCalledWith({
      name: 'Dr. Carlos',
      crm: '654321',
      councilState: 'RJ',
      specialty: 'Neurologia',
    });
    expect(fixture.componentInstance.successMessage()).toBeTruthy();
  }));
});
