import React from 'react';
import { StyleSheet, View } from 'react-native';
import { Text } from 'react-native-paper';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { AppButton } from '../../components/ui/AppButton';
import { colors, spacing } from '../../theme';

export function MagicLinkSentScreen() {
  const router = useRouter();
  const { email } = useLocalSearchParams<{ email: string }>();

  return (
    <View style={styles.container}>
      <Text variant="headlineMedium" style={styles.title}>
        Verifique seu email
      </Text>

      <Text style={styles.description}>
        Enviamos um link de acesso para:
      </Text>

      <Text style={styles.email}>{email}</Text>

      <Text style={styles.info}>
        O link é válido por 15 minutos. Verifique também sua caixa de spam.
      </Text>

      <AppButton label="Voltar ao Login" onPress={() => router.back()} />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
    padding: spacing.lg,
    justifyContent: 'center',
    gap: spacing.md,
  },
  title: {
    color: colors.textPrimary,
    fontWeight: '700',
    textAlign: 'center',
  },
  description: {
    color: colors.textSecondary,
    textAlign: 'center',
    fontSize: 16,
  },
  email: {
    color: colors.primary,
    fontWeight: '600',
    fontSize: 16,
    textAlign: 'center',
  },
  info: {
    color: colors.textSecondary,
    textAlign: 'center',
    fontSize: 14,
  },
});
