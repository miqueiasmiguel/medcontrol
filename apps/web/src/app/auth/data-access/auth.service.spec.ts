import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AuthService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AuthService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('should POST to /api/auth/magic-link/send with credentials', () => {
    service.sendMagicLink('test@test.com').subscribe();
    const req = httpTesting.expectOne('/api/auth/magic-link/send');
    expect(req.request.withCredentials).toBe(true);
    expect(req.request.body).toEqual({ email: 'test@test.com' });
    req.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('should POST to /api/auth/magic-link/verify with credentials', () => {
    service.verifyMagicLink('some-token').subscribe();
    const req = httpTesting.expectOne('/api/auth/magic-link/verify');
    expect(req.request.withCredentials).toBe(true);
    expect(req.request.body).toEqual({ token: 'some-token' });
    req.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('should POST to /api/auth/google/callback with credentials', () => {
    service.loginWithGoogle('auth-code', 'http://localhost:4200/auth/callback').subscribe();
    const req = httpTesting.expectOne('/api/auth/google/callback');
    expect(req.request.withCredentials).toBe(true);
    expect(req.request.body).toEqual({ code: 'auth-code', redirectUri: 'http://localhost:4200/auth/callback' });
    req.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('should POST to /api/auth/logout with credentials', () => {
    service.logout().subscribe();
    const req = httpTesting.expectOne('/api/auth/logout');
    expect(req.request.withCredentials).toBe(true);
    req.flush(null, { status: 204, statusText: 'No Content' });
  });
});
