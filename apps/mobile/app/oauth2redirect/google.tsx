import { useEffect } from 'react';
import { ActivityIndicator, StyleSheet, View } from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { makeRedirectUri } from 'expo-auth-session';
import Constants from 'expo-constants';
import { AuthService } from '../../src/services/auth.service';
import { useAuth } from '../../src/hooks/useAuth';
import { colors } from '../../src/theme';

export default function GoogleOAuthCallback() {
  const { code } = useLocalSearchParams<{ code?: string }>();
  const router = useRouter();
  const { setSession } = useAuth();

  const androidClientId =
    (Constants.expoConfig?.extra?.googleAndroidClientId as string | undefined) ?? '';
  const reverseClientId = androidClientId.split('.').reverse().join('.');
  const redirectUri = makeRedirectUri({
    native: `${reverseClientId}:/oauth2redirect/google`,
  });

  useEffect(() => {
    if (!code) {
      router.replace('/(auth)/login');
      return;
    }

    AuthService.loginWithGoogle(code, redirectUri)
      .then(async () => {
        await setSession(true);
        router.replace('/(app)');
      })
      .catch(() => {
        router.replace('/(auth)/login');
      });
  }, []);

  return (
    <View style={styles.container} testID="google-callback-loading">
      <ActivityIndicator size="large" color={colors.primary} />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: colors.background,
  },
});
