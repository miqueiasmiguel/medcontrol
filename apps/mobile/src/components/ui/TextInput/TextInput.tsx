import React from 'react';
import {
  View,
  Text,
  TextInput as RNTextInput,
  TextInputProps as RNTextInputProps,
  StyleSheet,
} from 'react-native';
import { colors, fontSizes, fontWeights, spacing, radius } from '../../../theme';

export interface TextInputProps extends RNTextInputProps {
  label: string;
  error?: string;
}

export function TextInput({ label, error, style, ...rest }: TextInputProps) {
  return (
    <View style={styles.container}>
      <Text style={styles.label}>{label}</Text>
      <RNTextInput
        style={[styles.input, error ? styles.inputError : styles.inputDefault, style]}
        placeholderTextColor={colors.neutral[400]}
        {...rest}
      />
      {error ? (
        <Text testID="input-error" style={styles.error}>
          {error}
        </Text>
      ) : null}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    marginBottom: spacing[4],
  },
  label: {
    fontSize: fontSizes.sm,
    fontWeight: fontWeights.medium,
    color: colors.neutral[700],
    marginBottom: spacing[1],
  },
  input: {
    height: 48,
    borderWidth: 1,
    borderRadius: radius.md,
    paddingHorizontal: spacing[4],
    fontSize: fontSizes.md,
    color: colors.neutral[900],
    backgroundColor: colors.neutral[0],
  },
  inputDefault: {
    borderColor: colors.neutral[300],
  },
  inputError: {
    borderColor: colors.error,
  },
  error: {
    marginTop: spacing[1],
    fontSize: fontSizes.xs,
    color: colors.error,
  },
});
