import React from 'react';
import { ActivityIndicator, StyleSheet, TouchableOpacity, View, StyleProp, ViewStyle } from 'react-native';
import { Button, Text } from 'react-native-paper';
import { useAppTheme } from '../../contexts/ThemeContext';
import type { Theme, DarkTheme } from '@medcontrol/design-system/native';

interface AppButtonProps {
  label?: string;
  children?: React.ReactNode;
  onPress: () => void;
  disabled?: boolean;
  loading?: boolean;
  variant?: 'filled' | 'outline';
  leftIcon?: React.ReactNode;
  testID?: string;
  style?: StyleProp<ViewStyle>;
}

export function AppButton({ label, children, onPress, disabled = false, loading = false, variant = 'filled', leftIcon, testID, style }: AppButtonProps) {
  const t = useAppTheme();
  const styles = createStyles(t);
  const content = children ?? label ?? '';

  if (loading) {
    return (
      <View testID={testID} style={[styles.container, styles.filled, style]}>
        <ActivityIndicator testID="app-button-loading" color={t.colors.primaryText} />
      </View>
    );
  }

  if (variant === 'outline') {
    return (
      <TouchableOpacity
        testID={testID}
        style={[styles.container, styles.outline, style]}
        onPress={disabled ? undefined : onPress}
        disabled={disabled}
        accessibilityRole="button"
      >
        {leftIcon ? <View style={styles.iconSlot}>{leftIcon}</View> : null}
        <Text style={[styles.outlineText, disabled && styles.disabledText]}>{content}</Text>
      </TouchableOpacity>
    );
  }

  return (
    <Button
      testID={testID}
      mode="contained"
      onPress={onPress}
      disabled={disabled}
      buttonColor={t.colors.primary}
      textColor={t.colors.primaryText}
      style={[styles.container, style]}
    >
      {content}
    </Button>
  );
}

function createStyles(t: Theme | DarkTheme) {
  return StyleSheet.create({
    container: {
      borderRadius: 8,
      paddingVertical: 4,
    },
    filled: {
      backgroundColor: t.colors.primary,
      alignItems: 'center',
      justifyContent: 'center',
      height: 48,
    },
    outline: {
      borderWidth: 1.5,
      borderColor: t.colors.borderStrong,
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
      color: t.colors.text.primary,
      fontWeight: '600',
      fontSize: 14,
    },
    disabledText: {
      color: t.colors.text.disabled,
    },
  });
}
