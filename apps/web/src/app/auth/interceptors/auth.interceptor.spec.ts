import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
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
});
