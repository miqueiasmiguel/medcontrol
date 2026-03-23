import React, { useEffect, useState } from 'react';
import { ActivityIndicator, StyleSheet, View } from 'react-native';
import { Text } from 'react-native-paper';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { AuthService } from '../../services/auth.service';
import { AppButton } from '../../components/ui/AppButton';
import { colors, spacing } from '../../theme';

export function MagicLinkVerifyScreen() {
  const router = useRouter();
  const { token } = useLocalSearchParams<{ token?: string }>();
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (!token) {
      setError('Link inválido. Solicite um novo.');
      setIsLoading(false);
      return;
    }

    AuthService.verifyMagicLink(token)
      .then(() => router.replace('/(app)'))
      .catch((err: Error) => {
        setError(err.message ?? 'Erro ao verificar o link');
        setIsLoading(false);
      });
  }, [token, router]);

  if (isLoading && !error) {
    return (
      <View style={styles.container}>
        <ActivityIndicator testID="verify-loading" size="large" color={colors.primary} />
        <Text style={styles.loadingText}>Verificando seu link...</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <Text style={styles.errorText}>{error}</Text>
      <AppButton label="Voltar ao Login" onPress={() => router.replace('/(auth)/login')} />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
    padding: spacing.lg,
    justifyContent: 'center',
    alignItems: 'center',
    gap: spacing.md,
  },
  loadingText: {
    color: colors.textSecondary,
    fontSize: 16,
    marginTop: spacing.md,
  },
  errorText: {
    color: colors.error,
    fontSize: 16,
    textAlign: 'center',
  },
});
