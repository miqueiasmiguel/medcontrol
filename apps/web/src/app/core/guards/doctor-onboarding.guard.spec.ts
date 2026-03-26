import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { doctorOnboardingGuard } from './doctor-onboarding.guard';
import { SettingsService, UserDto } from '../../settings/data-access/settings.service';
import { DoctorService, DoctorDto } from '../../doctors/data-access/doctor.service';
import { WINDOW } from '../tokens/window.token';

describe('doctorOnboardingGuard', () => {
  let settingsService: jest.Mocked<Pick<SettingsService, 'getMe'>>;
  let doctorService: jest.Mocked<Pick<DoctorService, 'getMyDoctorProfile'>>;
  let mockWindow: { sessionStorage: { getItem: jest.Mock; setItem: jest.Mock } };

  const doctorUser: UserDto = {
    id: 'user-1',
    email: 'dr@test.com',
    displayName: 'Dr. Test',
    avatarUrl: null,
    isEmailVerified: true,
    globalRole: 'None',
    lastLoginAt: null,
    tenantRole: 'doctor',
  };

  const operatorUser: UserDto = { ...doctorUser, tenantRole: 'operator' };

  const mockProfile: DoctorDto = {
    id: 'doc-1',
    tenantId: 'tenant-1',
    userId: 'user-1',
    name: 'Dr. Test',
    crm: '123456',
    councilState: 'SP',
    specialty: 'Cardiologia',
  };

  function setup() {
    settingsService = { getMe: jest.fn() };
    doctorService = { getMyDoctorProfile: jest.fn() };
    mockWindow = {
      sessionStorage: { getItem: jest.fn().mockReturnValue(null), setItem: jest.fn() },
    };

    TestBed.configureTestingModule({
      providers: [
        provideRouter([{ path: 'onboarding', children: [] }, { path: 'doctors', children: [] }]),
        { provide: SettingsService, useValue: settingsService },
        { provide: DoctorService, useValue: doctorService },
        { provide: WINDOW, useValue: mockWindow },
      ],
    });
  }

  function runGuard(): Promise<boolean | import('@angular/router').UrlTree> {
    return TestBed.runInInjectionContext(() => {
      const result = doctorOnboardingGuard({} as never, {} as never);
      if (typeof result === 'object' && 'subscribe' in result) {
        return new Promise((resolve) => result.subscribe((v) => resolve(v as boolean | import('@angular/router').UrlTree)));
      }
      return Promise.resolve(result as boolean | import('@angular/router').UrlTree);
    });
  }

  it('allows access when skip flag is set in sessionStorage', async () => {
    setup();
    mockWindow.sessionStorage.getItem.mockReturnValue('1');
    settingsService.getMe.mockReturnValue(of(doctorUser));

    const result = await runGuard();
    expect(result).toBe(true);
    expect(settingsService.getMe).not.toHaveBeenCalled();
  });

  it('allows access when user is not doctor role', async () => {
    setup();
    settingsService.getMe.mockReturnValue(of(operatorUser));

    const result = await runGuard();
    expect(result).toBe(true);
    expect(doctorService.getMyDoctorProfile).not.toHaveBeenCalled();
  });

  it('allows access when user is doctor with existing profile', async () => {
    setup();
    settingsService.getMe.mockReturnValue(of(doctorUser));
    doctorService.getMyDoctorProfile.mockReturnValue(of(mockProfile));

    const result = await runGuard();
    expect(result).toBe(true);
  });

  it('redirects to /onboarding when doctor has no profile', async () => {
    setup();
    settingsService.getMe.mockReturnValue(of(doctorUser));
    doctorService.getMyDoctorProfile.mockReturnValue(of(null));

    const result = await runGuard();
    const router = TestBed.inject(Router);
    expect(result).toEqual(router.createUrlTree(['/onboarding']));
  });

  it('allows access on getMe error', async () => {
    setup();
    settingsService.getMe.mockReturnValue(throwError(() => new Error('network')));

    const result = await runGuard();
    expect(result).toBe(true);
  });
});
