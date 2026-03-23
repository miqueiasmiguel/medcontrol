import { useEffect } from 'react';
import { ActivityIndicator, StyleSheet, View } from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import AsyncStorage from '@react-native-async-storage/async-storage';
import Constants from 'expo-constants';
import { AuthService } from '../../src/services/auth.service';
import { useAuth } from '../../src/hooks/useAuth';
import { colors } from '../../src/theme';

export default function GoogleOAuthCallback() {
  const { code } = useLocalSearchParams<{ code?: string }>();
  const router = useRouter();
  const { setSession } = useAuth();

  useEffect(() => {
    if (!code) {
      router.replace('/(auth)/login');
      return;
    }

    (async () => {
      try {
        const codeVerifier = await AsyncStorage.getItem('oauth_code_verifier');
        const storedRedirectUri = await AsyncStorage.getItem('oauth_redirect_uri');
        await AsyncStorage.multiRemove(['oauth_code_verifier', 'oauth_redirect_uri']);

        if (!codeVerifier || !storedRedirectUri) {
          throw new Error('Dados OAuth inválidos');
        }

        const androidClientId = Constants.expoConfig?.extra?.googleAndroidClientId as string;
        const tokenRes = await fetch('https://oauth2.googleapis.com/token', {
          method: 'POST',
          headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
          body: new URLSearchParams({
            code,
            client_id: androidClientId,
            code_verifier: codeVerifier,
            redirect_uri: storedRedirectUri,
            grant_type: 'authorization_code',
          }).toString(),
        });

        if (!tokenRes.ok) throw new Error('Falha na troca do código OAuth');
        const tokens = (await tokenRes.json()) as { id_token?: string };
        if (!tokens.id_token) throw new Error('id_token ausente na resposta');

        await AuthService.verifyGoogleIdToken(tokens.id_token);
        await setSession(true);
        router.replace('/(app)');
      } catch (err: unknown) {
        const msg = err instanceof Error ? err.message : 'Erro de autenticação com Google';
        router.replace({ pathname: '/(auth)/login', params: { error: msg } });
      }
    })();
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
