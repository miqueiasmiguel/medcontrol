import React from 'react';
import {
  TouchableOpacity,
  Text,
  ActivityIndicator,
  StyleSheet,
  TouchableOpacityProps,
} from 'react-native';
import { colors, fontSizes, fontWeights, spacing, radius } from '../../../theme';

export interface ButtonProps extends Omit<TouchableOpacityProps, 'onPress'> {
  label: string;
  onPress: () => void;
  variant?: 'primary' | 'ghost';
  loading?: boolean;
  disabled?: boolean;
}

export function Button({
  label,
  onPress,
  variant = 'primary',
  loading = false,
  disabled = false,
  testID,
  ...rest
}: ButtonProps) {
  const isPrimary = variant === 'primary';
  const isDisabled = disabled || loading;

  return (
    <TouchableOpacity
      testID={testID}
      onPress={isDisabled ? undefined : onPress}
      disabled={isDisabled}
      activeOpacity={0.75}
      style={[
        styles.base,
        isPrimary ? styles.primary : styles.ghost,
        isDisabled && styles.disabled,
      ]}
      {...rest}
    >
      {loading ? (
        <ActivityIndicator
          testID="button-loading"
          color={isPrimary ? colors.neutral[0] : colors.orange[500]}
          size="small"
        />
      ) : (
        <Text style={[styles.label, isPrimary ? styles.labelPrimary : styles.labelGhost]}>
          {label}
        </Text>
      )}
    </TouchableOpacity>
  );
}

const styles = StyleSheet.create({
  base: {
    height: 48,
    borderRadius: radius.md,
    justifyContent: 'center',
    alignItems: 'center',
    paddingHorizontal: spacing[6],
  },
  primary: {
    backgroundColor: colors.orange[500],
  },
  ghost: {
    backgroundColor: 'transparent',
    borderWidth: 1,
    borderColor: colors.neutral[300],
  },
  disabled: {
    opacity: 0.6,
  },
  label: {
    fontSize: fontSizes.md,
    fontWeight: fontWeights.semibold,
  },
  labelPrimary: {
    color: colors.neutral[0],
  },
  labelGhost: {
    color: colors.navy[900],
  },
});
