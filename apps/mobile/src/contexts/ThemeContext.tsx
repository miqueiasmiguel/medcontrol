import React, { createContext, useContext, useEffect, useState } from 'react';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { useColorScheme } from 'react-native';
import { theme, darkTheme } from '@medcontrol/design-system/native';

export type ThemePreference = 'light' | 'dark' | 'system';

const THEME_STORAGE_KEY = 'mmc_theme';

interface ThemeContextValue {
  preference: ThemePreference;
  setPreference: (p: ThemePreference) => void;
}

const ThemeContext = createContext<ThemeContextValue>({
  preference: 'system',
  setPreference: () => undefined,
});

export function ThemePreferenceProvider({ children }: { children: React.ReactNode }) {
  const [preference, setPreferenceState] = useState<ThemePreference>('system');

  useEffect(() => {
    void AsyncStorage.getItem(THEME_STORAGE_KEY).then((v) => {
      if (v === 'light' || v === 'dark' || v === 'system') {
        setPreferenceState(v);
      }
    });
  }, []);

  function setPreference(p: ThemePreference) {
    setPreferenceState(p);
    void AsyncStorage.setItem(THEME_STORAGE_KEY, p);
  }

  return (
    <ThemeContext.Provider value={{ preference, setPreference }}>
      {children}
    </ThemeContext.Provider>
  );
}

export function useThemePreference() {
  return useContext(ThemeContext);
}

export function useAppTheme() {
  const { preference } = useThemePreference();
  const systemScheme = useColorScheme();
  const resolved = preference === 'system' ? systemScheme : preference;
  return resolved === 'dark' ? darkTheme : theme;
}
