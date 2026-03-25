import { useCallback, useEffect, useState } from 'react';
import { type UserDto, UserService } from '../services/user.service';

export interface UseCurrentUserResult {
  user: UserDto | null;
  loading: boolean;
  error: string | null;
}

export function useCurrentUser(): UseCurrentUserResult {
  const [user, setUser] = useState<UserDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchUser = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      setUser(await UserService.getMe());
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Erro ao carregar usuário');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void fetchUser();
  }, [fetchUser]);

  return { user, loading, error };
}
