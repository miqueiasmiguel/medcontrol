import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface MemberDto {
  userId: string;
  displayName: string | null;
  email: string | null;
  avatarUrl: string | null;
  role: string;
  joinedAt: string;
}

export interface AddMemberCommand {
  email: string;
  role: string;
}

export interface UpdateMemberRoleCommand {
  role: string;
}

@Injectable({ providedIn: 'root' })
export class MembersService {
  private readonly http = inject(HttpClient);

  getMembers() {
    return this.http.get<MemberDto[]>('/api/members', { withCredentials: true });
  }

  addMember(cmd: AddMemberCommand) {
    return this.http.post<MemberDto>('/api/members', cmd, { withCredentials: true });
  }

  updateMemberRole(userId: string, cmd: UpdateMemberRoleCommand) {
    return this.http.patch<MemberDto>(`/api/members/${userId}`, cmd, { withCredentials: true });
  }

  removeMember(userId: string) {
    return this.http.delete<void>(`/api/members/${userId}`, { withCredentials: true });
  }
}
