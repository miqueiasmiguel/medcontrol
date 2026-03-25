import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpTesting: HttpTestingController;
  let router: { navigate: jest.Mock };

  beforeEach(() => {
    router = { navigate: jest.fn() };

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: Router, useValue: router },
      ],
    });
    http = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('should set withCredentials on requests to /api', () => {
    http.get('/api/users/me').subscribe();
    const req = httpTesting.expectOne('/api/users/me');
    expect(req.request.withCredentials).toBe(true);
    req.flush(null);
  });

  it('should not set withCredentials on external requests', () => {
    http.get('https://fonts.googleapis.com').subscribe();
    const req = httpTesting.expectOne('https://fonts.googleapis.com');
    expect(req.request.withCredentials).toBe(false);
    req.flush(null);
  });

  it('should navigate to /auth/login on 401 from /api resource', () => {
    http.get('/api/doctors').subscribe({ error: () => {} });
    const req = httpTesting.expectOne('/api/doctors');
    req.flush({ message: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });

    expect(router.navigate).toHaveBeenCalledWith(['/auth/login']);
  });

  it('should not navigate to /auth/login on 401 from /api/auth endpoint', () => {
    http.post('/api/auth/magic-link/verify', {}).subscribe({ error: () => {} });
    const req = httpTesting.expectOne('/api/auth/magic-link/verify');
    req.flush({ message: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });

    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('should not navigate to /auth/login on other errors', () => {
    http.get('/api/doctors').subscribe({ error: () => {} });
    const req = httpTesting.expectOne('/api/doctors');
    req.flush({ message: 'Server Error' }, { status: 500, statusText: 'Internal Server Error' });

    expect(router.navigate).not.toHaveBeenCalled();
  });
});
