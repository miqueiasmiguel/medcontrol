import React, { useEffect } from 'react';
import { Stack, useRouter, useSegments } from 'expo-router';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { useAuth } from '../../src/hooks/useAuth';
import { useCurrentUser } from '../../src/hooks/useCurrentUser';
import { useDoctorProfile } from '../../src/hooks/useDoctorProfile';

export default function AppLayout() {
  const router = useRouter();
  const segments = useSegments();
  const { isAuthenticated, isLoading } = useAuth();
  const { user, loading: userLoading } = useCurrentUser();
  const { doctorProfile, loading: profileLoading } = useDoctorProfile();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.replace('/(auth)/login');
      return;
    }
  }, [isAuthenticated, isLoading, router]);

  useEffect(() => {
    if (isLoading || userLoading || profileLoading) return;
    if (!isAuthenticated) return;
    if (user?.tenantRole !== 'doctor') return;
    if (doctorProfile !== null) return;

    const currentSegment = String(segments[segments.length - 1]);
    if (currentSegment === 'doctor-onboarding') return;

    void AsyncStorage.getItem('mmc_onboarding_skip').then((skip) => {
      if (!skip) {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        router.replace('/(app)/doctor-onboarding' as any);
      }
    });
  }, [isAuthenticated, isLoading, userLoading, profileLoading, user, doctorProfile, segments, router]);

  return (
    <Stack screenOptions={{ headerShown: false }} />
  );
}
