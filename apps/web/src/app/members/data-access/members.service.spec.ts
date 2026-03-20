import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { MembersService, AddMemberCommand, UpdateMemberRoleCommand } from './members.service';

describe('MembersService', () => {
  let service: MembersService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [MembersService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(MembersService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('should GET /api/members with credentials', () => {
    service.getMembers().subscribe();
    const req = httpTesting.expectOne('/api/members');
    expect(req.request.method).toBe('GET');
    expect(req.request.withCredentials).toBe(true);
    req.flush([]);
  });

  it('should POST /api/members with command and credentials', () => {
    const command: AddMemberCommand = { email: 'new@example.com', role: 'operator' };

    service.addMember(command).subscribe();

    const req = httpTesting.expectOne('/api/members');
    expect(req.request.method).toBe('POST');
    expect(req.request.withCredentials).toBe(true);
    expect(req.request.body).toEqual(command);
    req.flush({ userId: 'u-1', ...command, displayName: null, avatarUrl: null, joinedAt: new Date().toISOString() });
  });

  it('should PATCH /api/members/:userId with command and credentials', () => {
    const command: UpdateMemberRoleCommand = { role: 'doctor' };

    service.updateMemberRole('u-1', command).subscribe();

    const req = httpTesting.expectOne('/api/members/u-1');
    expect(req.request.method).toBe('PATCH');
    expect(req.request.withCredentials).toBe(true);
    expect(req.request.body).toEqual(command);
    req.flush({ userId: 'u-1', role: 'doctor', displayName: null, email: null, avatarUrl: null, joinedAt: new Date().toISOString() });
  });

  it('should DELETE /api/members/:userId with credentials', () => {
    service.removeMember('u-1').subscribe();

    const req = httpTesting.expectOne('/api/members/u-1');
    expect(req.request.method).toBe('DELETE');
    expect(req.request.withCredentials).toBe(true);
    req.flush(null);
  });
});
