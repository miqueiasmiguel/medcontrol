import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface ProcedureDto {
  id: string;
  code: string;
  description: string;
  value: number;
  effectiveFrom: string;
  effectiveTo: string | null;
  source: string;
}

export interface CreateProcedureCommand {
  code: string;
  description: string;
  value: number;
  effectiveFrom: string;
  effectiveTo?: string;
}

export interface UpdateProcedureCommand {
  code: string;
  description: string;
  value: number;
  effectiveTo?: string;
}

export interface ProcedureImportDto {
  id: string;
  source: string;
  effectiveFrom: string;
  totalRows: number;
  importedRows: number;
  skippedRows: number;
  errorSummary: string | null;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class ProcedureService {
  private readonly http = inject(HttpClient);

  getProcedures(activeOnly = true) {
    return this.http.get<ProcedureDto[]>(`/api/procedures?activeOnly=${activeOnly}`, {
      withCredentials: true,
    });
  }

  createProcedure(command: CreateProcedureCommand) {
    return this.http.post<ProcedureDto>('/api/procedures', command, { withCredentials: true });
  }

  updateProcedure(id: string, command: UpdateProcedureCommand) {
    return this.http.patch<ProcedureDto>(`/api/procedures/${id}`, command, {
      withCredentials: true,
    });
  }

  importProcedures(file: File, source: string, effectiveFrom: string) {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('source', source);
    formData.append('effectiveFrom', effectiveFrom);
    return this.http.post<ProcedureImportDto>('/api/procedures/import', formData, {
      withCredentials: true,
    });
  }

  getImports() {
    return this.http.get<ProcedureImportDto[]>('/api/procedures/imports', {
      withCredentials: true,
    });
  }
}
