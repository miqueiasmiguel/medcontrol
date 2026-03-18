import { SessionService } from './session.service';

describe('SessionService', () => {
  let service: SessionService;

  beforeEach(() => {
    service = new SessionService();
  });

  afterEach(() => {
    document.cookie = 'mmc_session=; Max-Age=0; Path=/';
  });

  it('should return true when mmc_session cookie is present', () => {
    document.cookie = 'mmc_session=1; Path=/';
    expect(service.isAuthenticated()).toBe(true);
  });

  it('should return false when mmc_session cookie is absent', () => {
    expect(service.isAuthenticated()).toBe(false);
  });
});
