import { useCallback, useEffect, useState } from 'react';
import { type DoctorProfileDto, UserService } from '../services/user.service';

export interface UseDoctorProfileResult {
  doctorProfile: DoctorProfileDto | null;
  loading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
}

export function useDoctorProfile(): UseDoctorProfileResult {
  const [doctorProfile, setDoctorProfile] = useState<DoctorProfileDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchProfile = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      setDoctorProfile(await UserService.getDoctorProfile());
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Erro ao carregar perfil do médico');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void fetchProfile();
  }, [fetchProfile]);

  return { doctorProfile, loading, error, refetch: fetchProfile };
}
