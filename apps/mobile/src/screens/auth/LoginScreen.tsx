import React, { useState } from 'react';
import {
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  StyleSheet,
  View,
} from 'react-native';
import { Text } from 'react-native-paper';
import { SafeAreaView } from 'react-native-safe-area-context';
import { AntDesign } from '@expo/vector-icons';
import { useRouter, useLocalSearchParams } from 'expo-router';
import { useAuthRequest, makeRedirectUri, ResponseType } from 'expo-auth-session';
import AsyncStorage from '@react-native-async-storage/async-storage';
import Constants from 'expo-constants';
import { useForm, Controller } from 'react-hook-form';
import { AppButton } from '../../components/ui/AppButton';
import { AppTextInput } from '../../components/ui/AppTextInput';
import { AuthService } from '../../services/auth.service';
import { colors, spacing, typography } from '../../theme';

const GOOGLE_PKCE_DISCOVERY = {
  authorizationEndpoint: 'https://accounts.google.com/o/oauth2/v2/auth',
  tokenEndpoint: 'https://oauth2.googleapis.com/token',
};

interface LoginForm {
  email: string;
}

export function LoginScreen() {
  const router = useRouter();
  const { error: oauthError } = useLocalSearchParams<{ error?: string }>();
  const [apiError, setApiError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginForm>({ defaultValues: { email: '' } });

  const androidClientId =
    (Constants.expoConfig?.extra?.googleAndroidClientId as string | undefined) ?? '';
  const redirectUri = makeRedirectUri({
    native: `${androidClientId.split('.').reverse().join('.')}:/oauth2redirect/google`,
  });

  const [request, , promptAsync] = useAuthRequest(
    {
      clientId: androidClientId,
      redirectUri,
      responseType: ResponseType.Code,
      usePKCE: true,
      scopes: ['openid', 'profile', 'email'],
    },
    GOOGLE_PKCE_DISCOVERY,
  );

  const handleGooglePress = async () => {
    if (request?.codeVerifier) {
      await AsyncStorage.setItem('oauth_code_verifier', request.codeVerifier);
      await AsyncStorage.setItem('oauth_redirect_uri', redirectUri);
    }
    promptAsync();
  };

  const onSubmitEmail = handleSubmit(async ({ email }) => {
    setApiError(null);
    setIsSubmitting(true);
    try {
      await AuthService.sendMagicLink(email);
      router.push({ pathname: '/(auth)/magic-link-sent', params: { email } });
    } catch (err) {
      setApiError(err instanceof Error ? err.message : 'Erro desconhecido');
    } finally {
      setIsSubmitting(false);
    }
  });

  return (
    <SafeAreaView style={styles.safe} edges={['top']}>
      <KeyboardAvoidingView
        style={styles.flex}
        behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
      >
        <ScrollView
          contentContainerStyle={styles.scroll}
          keyboardShouldPersistTaps="handled"
          bounces={false}
          showsVerticalScrollIndicator={false}
        >
          {/* Hero — fundo navy escuro com identidade da marca */}
          <View style={styles.hero}>
            <View style={styles.logoBadge}>
              <Text style={styles.logoBadgeText}>M</Text>
            </View>
            <Text style={styles.wordmark}>
              Med<Text style={styles.wordmarkAccent}>Control</Text>
            </Text>
            <Text style={styles.heroTagline}>Gestão médica simplificada</Text>
          </View>

          {/* Card do formulário — flutua sobre o hero */}
          <View style={styles.card}>
            <Text style={styles.cardTitle}>Entrar na sua conta</Text>
            <Text style={styles.cardSubtitle}>Use seu e-mail ou conta Google</Text>

            <View style={styles.form}>
              <Controller
                control={control}
                name="email"
                rules={{
                  required: 'Email é obrigatório',
                  pattern: { value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/, message: 'Email inválido' },
                }}
                render={({ field: { onChange, value } }) => (
                  <AppTextInput
                    label="Email"
                    value={value}
                    onChangeText={onChange}
                    placeholder="seu@email.com"
                    keyboardType="email-address"
                    autoCapitalize="none"
                    autoComplete="email"
                    errorMessage={errors.email?.message}
                  />
                )}
              />

              {(apiError ?? oauthError) ? (
                <Text style={styles.apiError}>{apiError ?? oauthError}</Text>
              ) : null}

              <AppButton
                label="Continuar com Email"
                onPress={onSubmitEmail}
                loading={isSubmitting}
              />

              <View style={styles.divider}>
                <View style={styles.dividerLine} />
                <Text style={styles.dividerText}>ou</Text>
                <View style={styles.dividerLine} />
              </View>

              <AppButton
                label="Continuar com Google"
                onPress={handleGooglePress}
                variant="outline"
                leftIcon={<AntDesign name="google" size={18} color={colors.navy} />}
              />
            </View>
          </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: {
    flex: 1,
    backgroundColor: colors.navyDark,
  },
  flex: {
    flex: 1,
  },
  scroll: {
    flexGrow: 1,
  },

  // ── Hero ──────────────────────────────────────────────────────────────────
  hero: {
    backgroundColor: colors.navyDark,
    paddingTop: spacing.xxl,
    paddingBottom: spacing.xxl + spacing.lg,
    paddingHorizontal: spacing.lg,
    alignItems: 'center',
  },
  logoBadge: {
    width: 60,
    height: 60,
    borderRadius: 18,
    backgroundColor: colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: spacing.md,
    shadowColor: colors.primary,
    shadowOffset: { width: 0, height: 6 },
    shadowOpacity: 0.45,
    shadowRadius: 14,
    elevation: 8,
  },
  logoBadgeText: {
    fontSize: 30,
    fontWeight: '800',
    color: colors.white,
    letterSpacing: -0.5,
  },
  wordmark: {
    fontSize: typography.sizes.xl,
    fontWeight: '700',
    color: colors.white,
    letterSpacing: 0.3,
    marginBottom: spacing.xs,
  },
  wordmarkAccent: {
    color: colors.primary,
  },
  heroTagline: {
    fontSize: typography.sizes.sm,
    color: 'rgba(255, 255, 255, 0.50)',
    letterSpacing: 0.4,
  },

  // ── Card ──────────────────────────────────────────────────────────────────
  card: {
    flex: 1,
    backgroundColor: colors.surface,
    borderTopLeftRadius: 28,
    borderTopRightRadius: 28,
    paddingTop: spacing.xl,
    paddingHorizontal: spacing.lg,
    paddingBottom: spacing.xxl,
    marginTop: -spacing.lg,
    shadowColor: colors.navyDark,
    shadowOffset: { width: 0, height: -6 },
    shadowOpacity: 0.14,
    shadowRadius: 20,
    elevation: 14,
  },
  cardTitle: {
    fontSize: typography.sizes.xl,
    fontWeight: '700',
    color: colors.navy,
    marginBottom: spacing.xs,
  },
  cardSubtitle: {
    fontSize: typography.sizes.sm,
    color: colors.textSecondary,
    marginBottom: spacing.lg,
  },

  // ── Formulário ────────────────────────────────────────────────────────────
  form: {
    gap: spacing.md,
  },
  apiError: {
    color: colors.error,
    fontSize: typography.sizes.sm,
    textAlign: 'center',
  },
  divider: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.sm,
  },
  dividerLine: {
    flex: 1,
    height: 1,
    backgroundColor: colors.border,
  },
  dividerText: {
    color: colors.textSecondary,
    fontSize: typography.sizes.sm,
  },
});
