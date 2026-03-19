import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface ProcedureDto {
  id: string;
  code: string;
  description: string;
  value: number;
}

export interface CreateProcedureCommand {
  code: string;
  description: string;
  value: number;
}

export interface UpdateProcedureCommand {
  code: string;
  description: string;
  value: number;
}

@Injectable({ providedIn: 'root' })
export class ProcedureService {
  private readonly http = inject(HttpClient);

  getProcedures() {
    return this.http.get<ProcedureDto[]>('/api/procedures', { withCredentials: true });
  }

  createProcedure(command: CreateProcedureCommand) {
    return this.http.post<ProcedureDto>('/api/procedures', command, { withCredentials: true });
  }

  updateProcedure(id: string, command: UpdateProcedureCommand) {
    return this.http.patch<ProcedureDto>(`/api/procedures/${id}`, command, { withCredentials: true });
  }
}
