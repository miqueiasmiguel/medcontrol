import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { SettingsService, UpdateProfileRequest } from './settings.service';

describe('SettingsService', () => {
  let service: SettingsService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [SettingsService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(SettingsService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('should GET /api/users/me with credentials', () => {
    service.getMe().subscribe();
    const req = httpTesting.expectOne('/api/users/me');
    expect(req.request.method).toBe('GET');
    expect(req.request.withCredentials).toBe(true);
    req.flush({ id: 'u1', email: 'user@example.com', displayName: null, avatarUrl: null, isEmailVerified: false, globalRole: 'None', lastLoginAt: null });
  });

  it('should PATCH /api/users/me/profile with request and credentials', () => {
    const request: UpdateProfileRequest = { displayName: 'João Silva' };
    service.updateProfile(request).subscribe();
    const req = httpTesting.expectOne('/api/users/me/profile');
    expect(req.request.method).toBe('PATCH');
    expect(req.request.withCredentials).toBe(true);
    expect(req.request.body).toEqual(request);
    req.flush({ id: 'u1', email: 'user@example.com', displayName: 'João Silva', avatarUrl: null, isEmailVerified: false, globalRole: 'None', lastLoginAt: null });
  });
});
