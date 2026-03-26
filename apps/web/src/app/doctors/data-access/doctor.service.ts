import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface DoctorDto {
  id: string;
  tenantId: string;
  userId: string | null;
  name: string;
  crm: string;
  councilState: string;
  specialty: string;
}

export interface CreateDoctorCommand {
  name: string;
  crm: string;
  councilState: string;
  specialty: string;
  inviteEmail?: string;
}

export interface UpdateDoctorCommand {
  name: string;
  crm: string;
  councilState: string;
  specialty: string;
}

@Injectable({ providedIn: 'root' })
export class DoctorService {
  private readonly http = inject(HttpClient);

  getDoctors() {
    return this.http.get<DoctorDto[]>('/api/doctors', { withCredentials: true });
  }

  createDoctor(command: CreateDoctorCommand) {
    return this.http.post<DoctorDto>('/api/doctors', command, { withCredentials: true });
  }

  updateDoctor(id: string, command: UpdateDoctorCommand) {
    return this.http.patch<DoctorDto>(`/api/doctors/${id}`, command, { withCredentials: true });
  }

  linkDoctorToUser(doctorId: string, userId: string) {
    return this.http.post<DoctorDto>(`/api/doctors/${doctorId}/link-user`, { userId }, { withCredentials: true });
  }

  inviteAndLinkMember(doctorId: string, email: string) {
    return this.http.post<DoctorDto>(`/api/doctors/${doctorId}/invite-and-link`, { email }, { withCredentials: true });
  }

  createMyDoctorProfile(command: { name: string; crm: string; councilState: string; specialty: string }) {
    return this.http.post<DoctorDto>('/api/users/me/doctor-profile', command, { withCredentials: true });
  }
}
