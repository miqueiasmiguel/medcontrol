import React from 'react';
import { ActivityIndicator, StyleSheet, TouchableOpacity, View } from 'react-native';
import { Button, Text } from 'react-native-paper';
import { colors } from '../../theme/colors';

interface AppButtonProps {
  label: string;
  onPress: () => void;
  disabled?: boolean;
  loading?: boolean;
  variant?: 'filled' | 'outline';
  leftIcon?: React.ReactNode;
}

export function AppButton({ label, onPress, disabled = false, loading = false, variant = 'filled', leftIcon }: AppButtonProps) {
  if (loading) {
    return (
      <View style={[styles.container, styles.filled]}>
        <ActivityIndicator testID="app-button-loading" color={colors.white} />
      </View>
    );
  }

  if (variant === 'outline') {
    return (
      <TouchableOpacity
        style={[styles.container, styles.outline]}
        onPress={disabled ? undefined : onPress}
        disabled={disabled}
        accessibilityRole="button"
      >
        {leftIcon ? <View style={styles.iconSlot}>{leftIcon}</View> : null}
        <Text style={[styles.outlineText, disabled && styles.disabledText]}>{label}</Text>
      </TouchableOpacity>
    );
  }

  return (
    <Button
      mode="contained"
      onPress={onPress}
      disabled={disabled}
      buttonColor={colors.primary}
      textColor={colors.white}
      style={styles.container}
    >
      {label}
    </Button>
  );
}

const styles = StyleSheet.create({
  container: {
    borderRadius: 8,
    paddingVertical: 4,
  },
  filled: {
    backgroundColor: colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
    height: 48,
  },
  outline: {
    borderWidth: 1.5,
    borderColor: colors.borderStrong,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    height: 48,
    gap: 8,
  },
  iconSlot: {
    width: 20,
    height: 20,
    alignItems: 'center',
    justifyContent: 'center',
  },
  outlineText: {
    color: colors.navy,
    fontWeight: '600',
    fontSize: 14,
  },
  disabledText: {
    color: colors.textSecondary,
  },
});
