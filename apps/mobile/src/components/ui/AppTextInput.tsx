import React from 'react';
import { StyleSheet, View } from 'react-native';
import { HelperText, TextInput } from 'react-native-paper';
import { colors } from '../../theme/colors';

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
}: AppTextInputProps) {
  return (
    <View style={styles.container}>
      <TextInput
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
        outlineColor={colors.border}
        activeOutlineColor={colors.primary}
        style={styles.input}
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
  input: {
    backgroundColor: colors.background,
  },
});
