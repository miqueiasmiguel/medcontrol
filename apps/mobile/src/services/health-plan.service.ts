import Constants from 'expo-constants';
import { emitUnauthorized } from '../lib/unauthorizedEmitter';

const API_BASE: string =
  (Constants.expoConfig?.extra?.apiUrl as string | undefined) ?? 'http://localhost:5000';

async function request<T>(path: string): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, {
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
  });

  if (!res.ok) {
    if (res.status === 401) emitUnauthorized();
    throw new Error(`HTTP ${res.status}`);
  }

  return res.json() as Promise<T>;
}

export interface HealthPlanDto {
  id: string;
  tenantId: string;
  name: string;
  tissCode: string;
}

export const HealthPlanService = {
  listHealthPlans: async (): Promise<HealthPlanDto[]> =>
    request<HealthPlanDto[]>('/health-plans'),
};
