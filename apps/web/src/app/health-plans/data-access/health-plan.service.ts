import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface HealthPlanDto {
  id: string;
  name: string;
  tissCode: string;
}

export interface CreateHealthPlanCommand {
  name: string;
  tissCode: string;
}

export interface UpdateHealthPlanCommand {
  name: string;
  tissCode: string;
}

@Injectable({ providedIn: 'root' })
export class HealthPlanService {
  private readonly http = inject(HttpClient);

  getHealthPlans() {
    return this.http.get<HealthPlanDto[]>('/api/health-plans', { withCredentials: true });
  }

  createHealthPlan(command: CreateHealthPlanCommand) {
    return this.http.post<HealthPlanDto>('/api/health-plans', command, { withCredentials: true });
  }

  updateHealthPlan(id: string, command: UpdateHealthPlanCommand) {
    return this.http.patch<HealthPlanDto>(`/api/health-plans/${id}`, command, { withCredentials: true });
  }
}
