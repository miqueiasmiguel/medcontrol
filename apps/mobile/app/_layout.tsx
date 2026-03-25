import React, { useEffect } from 'react';
import { Stack, useRouter, useSegments, useRootNavigationState } from 'expo-router';
import { PaperProvider } from 'react-native-paper';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { theme } from '../src/theme';
import { ThemePreferenceProvider } from '../src/contexts/ThemeContext';
import { useAuth } from '../src/hooks/useAuth';

function AuthGuard() {
  const { isAuthenticated, isLoading } = useAuth();
  const segments = useSegments();
  const router = useRouter();
  const navigationState = useRootNavigationState();

  useEffect(() => {
    if (!navigationState?.key || isLoading) return;

    const inAuthGroup = segments[0] === '(auth)';

    if (isAuthenticated && inAuthGroup) {
      router.replace('/(app)');
    } else if (!isAuthenticated && !inAuthGroup && segments[0] !== undefined) {
      router.replace('/(auth)/login');
    }
  }, [isAuthenticated, isLoading, segments, navigationState?.key, router]);

  return null;
}

export default function RootLayout() {
  return (
    <SafeAreaProvider>
      <ThemePreferenceProvider>
        <PaperProvider theme={theme}>
          <AuthGuard />
          <Stack screenOptions={{ headerShown: false }} />
        </PaperProvider>
      </ThemePreferenceProvider>
    </SafeAreaProvider>
  );
}
