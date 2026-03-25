import React from 'react';
import { StyleSheet, View } from 'react-native';
import { HelperText, TextInput } from 'react-native-paper';
import { useAppTheme } from '../../contexts/ThemeContext';

interface AppTextInputProps {
  label: string;
  value: string;
  onChangeText: (text: string) => void;
  errorMessage?: string;
  keyboardType?: 'default' | 'email-address';
  autoCapitalize?: 'none' | 'sentences' | 'words' | 'characters';
  autoComplete?: 'email' | 'off';
  placeholder?: string;
  secureTextEntry?: boolean;
  testID?: string;
}

export function AppTextInput({
  label,
  value,
  onChangeText,
  errorMessage,
  keyboardType = 'default',
  autoCapitalize = 'sentences',
  autoComplete,
  placeholder,
  secureTextEntry = false,
  testID,
}: AppTextInputProps) {
  const t = useAppTheme();

  return (
    <View style={styles.container}>
      <TextInput
        testID={testID}
        label={label}
        value={value}
        onChangeText={onChangeText}
        mode="outlined"
        keyboardType={keyboardType}
        autoCapitalize={autoCapitalize}
        autoComplete={autoComplete}
        placeholder={placeholder}
        secureTextEntry={secureTextEntry}
        error={!!errorMessage}
        outlineColor={t.colors.border}
        activeOutlineColor={t.colors.primary}
        textColor={t.colors.text.primary}
        placeholderTextColor={t.colors.text.tertiary}
        style={{ backgroundColor: t.colors.surface.card }}
        theme={{
          colors: {
            onSurfaceVariant: t.colors.text.secondary,
          },
        }}
      />
      {errorMessage ? (
        <HelperText type="error" visible>
          {errorMessage}
        </HelperText>
      ) : null}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    width: '100%',
  },
});
