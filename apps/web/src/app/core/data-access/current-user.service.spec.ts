import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { CurrentUserService } from './current-user.service';
import { UserDto } from '../../settings/data-access/settings.service';

const mockUser: UserDto = {
  id: 'user-1',
  email: 'test@example.com',
  displayName: 'Test User',
  avatarUrl: null,
  isEmailVerified: true,
  globalRole: 'None',
  lastLoginAt: null,
  tenantRole: 'operator',
};

const doctorUser: UserDto = { ...mockUser, tenantRole: 'doctor' };

describe('CurrentUserService', () => {
  let service: CurrentUserService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(CurrentUserService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('getMe() calls HTTP and returns the user', (done) => {
    service.getMe().subscribe((user) => {
      expect(user).toEqual(mockUser);
      done();
    });

    const req = httpMock.expectOne('/api/users/me');
    expect(req.request.method).toBe('GET');
    req.flush(mockUser);
  });

  it('getMe() caches the result — second call does not issue another HTTP request', (done) => {
    service.getMe().subscribe(() => {
      service.getMe().subscribe((user) => {
        expect(user).toEqual(mockUser);
        httpMock.expectNone('/api/users/me');
        done();
      });
    });

    httpMock.expectOne('/api/users/me').flush(mockUser);
  });

  it('invalidate() clears cache — next getMe() re-fetches', (done) => {
    service.getMe().subscribe(() => {
      service.invalidate();

      service.getMe().subscribe((user) => {
        expect(user).toEqual(mockUser);
        done();
      });

      httpMock.expectOne('/api/users/me').flush(mockUser);
    });

    httpMock.expectOne('/api/users/me').flush(mockUser);
  });

  it('isDoctor is true when tenantRole is doctor', (done) => {
    service.getMe().subscribe(() => {
      expect(service.isDoctor()).toBe(true);
      done();
    });

    httpMock.expectOne('/api/users/me').flush(doctorUser);
  });

  it('isDoctor is false when tenantRole is not doctor', (done) => {
    service.getMe().subscribe(() => {
      expect(service.isDoctor()).toBe(false);
      done();
    });

    httpMock.expectOne('/api/users/me').flush(mockUser);
  });
});
