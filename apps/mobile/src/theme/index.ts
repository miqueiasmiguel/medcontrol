import { MD3LightTheme, type MD3Theme } from 'react-native-paper';
import { colors } from './colors';

export { colors } from './colors';
export { spacing } from './spacing';
export { typography } from './typography';

export const theme: MD3Theme = {
  ...MD3LightTheme,
  colors: {
    ...MD3LightTheme.colors,
    primary: colors.primary,
    primaryContainer: colors.primaryLight,
    onPrimary: colors.white,
    secondary: colors.navy,
    onSecondary: colors.white,
    background: colors.background,
    surface: colors.surface,
    error: colors.error,
    onBackground: colors.textPrimary,
    onSurface: colors.textPrimary,
    outline: colors.border,
  },
};
