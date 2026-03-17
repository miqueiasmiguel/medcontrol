import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { guestGuard } from './guest.guard';
import { SessionService } from '../data-access/session.service';

describe('guestGuard', () => {
  let sessionService: { isAuthenticated: jest.Mock };
  let router: { createUrlTree: jest.Mock };

  beforeEach(() => {
    sessionService = { isAuthenticated: jest.fn() };
    router = { createUrlTree: jest.fn((c) => c) };

    TestBed.configureTestingModule({
      providers: [
        { provide: SessionService, useValue: sessionService },
        { provide: Router, useValue: router },
      ],
    });
  });

  it('should allow navigation when not authenticated', () => {
    sessionService.isAuthenticated.mockReturnValue(false);
    const result = TestBed.runInInjectionContext(() => guestGuard({} as never, {} as never));
    expect(result).toBe(true);
  });

  it('should redirect to / when already authenticated', () => {
    sessionService.isAuthenticated.mockReturnValue(true);
    TestBed.runInInjectionContext(() => guestGuard({} as never, {} as never));
    expect(router.createUrlTree).toHaveBeenCalledWith(['/']);
  });
});
