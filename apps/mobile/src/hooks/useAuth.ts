import { useCallback, useEffect, useState } from 'react';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { AuthService } from '../services/auth.service';
import { onUnauthorized } from '../lib/unauthorizedEmitter';

const SESSION_KEY = 'mmc_session';

export function useAuth() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    AsyncStorage.getItem(SESSION_KEY)
      .then((value) => setIsAuthenticated(value === '1'))
      .finally(() => setIsLoading(false));
  }, []);

  useEffect(() => {
    return onUnauthorized(() => {
      void setSession(false);
    });
  }, [setSession]);

  const setSession = useCallback(async (authenticated: boolean) => {
    if (authenticated) {
      await AsyncStorage.setItem(SESSION_KEY, '1');
    } else {
      await AsyncStorage.removeItem(SESSION_KEY);
    }
    setIsAuthenticated(authenticated);
  }, []);

  const logout = useCallback(async () => {
    await AuthService.logout();
    await AsyncStorage.removeItem(SESSION_KEY);
    setIsAuthenticated(false);
  }, []);

  return { isAuthenticated, isLoading, setSession, logout };
}
