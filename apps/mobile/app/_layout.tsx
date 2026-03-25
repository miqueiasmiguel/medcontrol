import React from 'react';
import { Stack } from 'expo-router';
import { PaperProvider } from 'react-native-paper';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { theme } from '../src/theme';
import { ThemePreferenceProvider } from '../src/contexts/ThemeContext';

export default function RootLayout() {
  return (
    <SafeAreaProvider>
      <ThemePreferenceProvider>
        <PaperProvider theme={theme}>
          <Stack screenOptions={{ headerShown: false }} />
        </PaperProvider>
      </ThemePreferenceProvider>
    </SafeAreaProvider>
  );
}
