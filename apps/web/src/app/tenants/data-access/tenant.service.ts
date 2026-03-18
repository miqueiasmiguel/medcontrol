import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface TenantDto {
  id: string;
  name: string;
  slug: string;
  role: string;
}

@Injectable({ providedIn: 'root' })
export class TenantService {
  private readonly http = inject(HttpClient);

  getMyTenants() {
    return this.http.get<TenantDto[]>('/api/tenants/me', { withCredentials: true });
  }

  createTenant(name: string) {
    return this.http.post('/api/tenants', { name }, { withCredentials: true });
  }

  switchTenant(tenantId: string) {
    return this.http.post('/api/tenants/switch', { tenantId }, { withCredentials: true });
  }
}
