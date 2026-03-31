import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AdminTenantDto {
  id: string;
  name: string;
  slug: string;
  isActive: boolean;
  createdAt: string;
  memberCount: number;
}

@Injectable({ providedIn: 'root' })
export class AdminTenantsService {
  private readonly http = inject(HttpClient);

  listTenants(): Observable<AdminTenantDto[]> {
    return this.http.get<AdminTenantDto[]>('/api/admin/tenants', { withCredentials: true });
  }

  createTenant(name: string, ownerEmail: string): Observable<AdminTenantDto> {
    return this.http.post<AdminTenantDto>(
      '/api/admin/tenants',
      { name, ownerEmail },
      { withCredentials: true },
    );
  }

  setTenantStatus(id: string, isActive: boolean): Observable<void> {
    return this.http.patch<void>(
      `/api/admin/tenants/${id}/status`,
      { isActive },
      { withCredentials: true },
    );
  }
}
