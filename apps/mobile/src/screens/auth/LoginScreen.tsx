import React, { useEffect, useState } from 'react';
import { StyleSheet, View } from 'react-native';
import { Text } from 'react-native-paper';
import { useRouter } from 'expo-router';
import * as Google from 'expo-auth-session/providers/google';
import { makeRedirectUri } from 'expo-auth-session';
import { useForm, Controller } from 'react-hook-form';
import { AppButton } from '../../components/ui/AppButton';
import { AppTextInput } from '../../components/ui/AppTextInput';
import { AuthService } from '../../services/auth.service';
import { colors, spacing } from '../../theme';

interface LoginForm {
  email: string;
}

export function LoginScreen() {
  const router = useRouter();
  const [apiError, setApiError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginForm>({ defaultValues: { email: '' } });

  const redirectUri = makeRedirectUri({ scheme: 'medcontrol', path: 'google-callback' });

  const [, googleResponse, promptAsync] = Google.useAuthRequest({
    clientId: '',
    redirectUri,
  });

  useEffect(() => {
    if (googleResponse?.type === 'success') {
      const { code } = googleResponse.params;
      AuthService.loginWithGoogle(code, redirectUri)
        .then(() => router.replace('/(app)'))
        .catch((err: Error) => setApiError(err.message));
    }
  }, [googleResponse, redirectUri, router]);

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
    <View style={styles.container}>
      <View style={styles.header}>
        <Text variant="headlineLarge" style={styles.title}>
          Bem-vindo
        </Text>
        <Text variant="bodyMedium" style={styles.subtitle}>
          Entre com seu email ou Google
        </Text>
      </View>

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

        {apiError ? <Text style={styles.apiError}>{apiError}</Text> : null}

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
          onPress={() => promptAsync()}
          variant="outline"
        />
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
    padding: spacing.lg,
    justifyContent: 'center',
  },
  header: {
    marginBottom: spacing.xl,
    alignItems: 'center',
  },
  title: {
    color: colors.textPrimary,
    fontWeight: '700',
    marginBottom: spacing.xs,
  },
  subtitle: {
    color: colors.textSecondary,
    textAlign: 'center',
  },
  form: {
    gap: spacing.md,
  },
  apiError: {
    color: colors.error,
    fontSize: 14,
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
    fontSize: 14,
  },
});
