import React from 'react';
import { View, Text, StyleSheet, SafeAreaView } from 'react-native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { RouteProp } from '@react-navigation/native';
import { AuthStackParamList } from '../../navigation/types';
import { Button } from '../../components/ui/Button/Button';
import { colors, fontSizes, fontWeights, spacing } from '../../theme';

type Props = {
  navigation: NativeStackNavigationProp<AuthStackParamList, 'MagicLinkSent'>;
  route: RouteProp<AuthStackParamList, 'MagicLinkSent'>;
};

export function MagicLinkSentScreen({ navigation, route }: Props) {
  const { email } = route.params;

  return (
    <SafeAreaView style={styles.safeArea}>
      <View style={styles.content}>
        <View style={styles.card}>
          <View style={styles.iconWrapper}>
            <Text style={styles.iconText}>✉</Text>
          </View>

          <Text style={styles.title}>Verifique seu e-mail</Text>
          <Text style={styles.description}>
            Enviamos um link de acesso para
          </Text>

          <View style={styles.emailPill}>
            <Text style={styles.emailText}>{email}</Text>
          </View>

          <Text style={styles.hint}>
            O link expira em 15 minutos. Verifique também a caixa de spam.
          </Text>

          <Button
            testID="btn-retry"
            label="Tentar novamente"
            onPress={() => navigation.goBack()}
            variant="ghost"
          />
        </View>
      </View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: colors.neutral[100],
  },
  content: {
    flex: 1,
    justifyContent: 'center',
    paddingHorizontal: spacing[5],
    paddingVertical: spacing[8],
  },
  card: {
    backgroundColor: colors.neutral[0],
    borderRadius: 12,
    padding: spacing[6],
    alignItems: 'center',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.08,
    shadowRadius: 8,
    elevation: 3,
  },
  iconWrapper: {
    width: 64,
    height: 64,
    borderRadius: 32,
    backgroundColor: colors.orange[100],
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: spacing[5],
  },
  iconText: {
    fontSize: 28,
  },
  title: {
    fontSize: fontSizes['2xl'],
    fontWeight: fontWeights.semibold,
    color: colors.neutral[900],
    marginBottom: spacing[2],
    textAlign: 'center',
  },
  description: {
    fontSize: fontSizes.sm,
    color: colors.neutral[500],
    textAlign: 'center',
    marginBottom: spacing[3],
  },
  emailPill: {
    borderWidth: 1,
    borderColor: colors.orange[200],
    backgroundColor: colors.orange[50],
    borderRadius: 9999,
    paddingHorizontal: spacing[4],
    paddingVertical: spacing[2],
    marginBottom: spacing[5],
  },
  emailText: {
    fontSize: fontSizes.sm,
    fontWeight: fontWeights.medium,
    color: colors.orange[700],
  },
  hint: {
    fontSize: fontSizes.xs,
    color: colors.neutral[400],
    textAlign: 'center',
    marginBottom: spacing[6],
    lineHeight: 18,
  },
});
