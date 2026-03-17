/**
 * MM Fature — React Native Theme
 *
 * Consumo:
 *   import { theme } from '@medcontrol/design-system/native';
 *   const styles = StyleSheet.create({ container: { backgroundColor: theme.colors.surface.background } });
 */

// ---------------------------------------------------------------------------
// Cores
// ---------------------------------------------------------------------------
const orange = {
  50: '#FFF4ED',
  100: '#FFE6D5',
  200: '#FFCBA9',
  300: '#FEA472',
  400: '#FD7A38',
  500: '#F97316',
  600: '#EA6310',
  700: '#C24E0D',
  800: '#9A3F11',
  900: '#7C3412',
  950: '#431807',
} as const;

const navy = {
  50: '#EEF2FF',
  100: '#E0E7FF',
  200: '#C7D2FE',
  300: '#A5B4FC',
  400: '#818CF8',
  500: '#6366F1',
  600: '#4F46E5',
  700: '#3730A3',
  800: '#2D2A8A',
  900: '#1B2E63',
  950: '#0F1A40',
} as const;

const neutral = {
  0: '#FFFFFF',
  50: '#F8F9FA',
  100: '#F1F3F5',
  200: '#E9ECEF',
  300: '#DEE2E6',
  400: '#CED4DA',
  500: '#ADB5BD',
  600: '#868E96',
  700: '#495057',
  800: '#343A40',
  900: '#212529',
  1000: '#0D0F12',
} as const;

// ---------------------------------------------------------------------------
// Theme object
// ---------------------------------------------------------------------------
export const theme = {
  colors: {
    // Marca
    orange,
    navy,
    neutral,

    // Primária (ação, botão principal)
    primary: orange[500],
    primaryHover: orange[600],
    primaryLight: orange[50],
    primaryText: neutral[0],

    // Secundária (navy)
    secondary: navy[900],
    secondaryLight: navy[50],
    secondaryText: neutral[0],

    // Superfícies
    surface: {
      background: neutral[50],
      card: neutral[0],
      cardPressed: neutral[100],
      overlay: 'rgba(15, 26, 64, 0.48)',

      // Bottom tab / nav escura (equivalente ao sidebar web)
      nav: navy[950],
      navActive: navy[900],
    },

    // Bordas
    border: neutral[200],
    borderStrong: neutral[300],
    divider: neutral[100],

    // Texto
    text: {
      primary: neutral[900],
      secondary: neutral[600],
      tertiary: neutral[500],
      disabled: neutral[400],
      inverse: neutral[0],
      onDark: 'rgba(255, 255, 255, 0.87)',
      onDarkSubtle: 'rgba(255, 255, 255, 0.55)',
      link: orange[600],
    },

    // Semânticas
    success: { light: '#ECFDF5', base: '#10B981', dark: '#065F46' },
    warning: { light: '#FFFBEB', base: '#F59E0B', dark: '#92400E' },
    error:   { light: '#FEF2F2', base: '#EF4444', dark: '#991B1B' },
    info:    { light: '#EFF6FF', base: '#3B82F6', dark: '#1E40AF' },

    // Status de pagamento
    paymentStatus: {
      pending: {
        bg: '#FFFBEB',
        text: '#92400E',
        border: '#FDE68A',
        dot: '#F59E0B',
      },
      paid: {
        bg: '#ECFDF5',
        text: '#065F46',
        border: '#A7F3D0',
        dot: '#10B981',
      },
      refused: {
        bg: '#FEF2F2',
        text: '#991B1B',
        border: '#FECACA',
        dot: '#EF4444',
      },
    },
  },

  // ---------------------------------------------------------------------------
  // Tipografia
  // ---------------------------------------------------------------------------
  typography: {
    // React Native usa fontFamily sem aspas
    fontFamily: {
      sans: 'Inter',
      mono: 'JetBrainsMono',
      // Fallback (sistema)
      sansFallback: undefined, // usa system font
    },

    fontWeight: {
      regular: '400' as const,
      medium: '500' as const,
      semibold: '600' as const,
      bold: '700' as const,
      extrabold: '800' as const,
    },

    fontSize: {
      xs: 12,
      sm: 14,
      md: 16,
      lg: 18,
      xl: 20,
      '2xl': 24,
      '3xl': 30,
      '4xl': 36,
      '5xl': 48,
    },

    lineHeight: {
      tight: 1.2,
      snug: 1.375,
      normal: 1.5,
      relaxed: 1.625,
    },
  },

  // ---------------------------------------------------------------------------
  // Espaçamento (base 4px)
  // ---------------------------------------------------------------------------
  spacing: {
    0: 0,
    0.5: 2,
    1: 4,
    1.5: 6,
    2: 8,
    2.5: 10,
    3: 12,
    3.5: 14,
    4: 16,
    5: 20,
    6: 24,
    7: 28,
    8: 32,
    9: 36,
    10: 40,
    12: 48,
    14: 56,
    16: 64,
    20: 80,
    24: 96,
  },

  // ---------------------------------------------------------------------------
  // Border Radius
  // ---------------------------------------------------------------------------
  borderRadius: {
    none: 0,
    xs: 2,
    sm: 4,
    md: 8,
    lg: 12,
    xl: 16,
    '2xl': 20,
    '3xl': 24,
    full: 9999,
  },

  // ---------------------------------------------------------------------------
  // Sombras (React Native usa elevation no Android + shadow* no iOS)
  // ---------------------------------------------------------------------------
  shadows: {
    none: {
      shadowColor: 'transparent',
      shadowOffset: { width: 0, height: 0 },
      shadowOpacity: 0,
      shadowRadius: 0,
      elevation: 0,
    },
    sm: {
      shadowColor: '#0F1A40',
      shadowOffset: { width: 0, height: 1 },
      shadowOpacity: 0.05,
      shadowRadius: 2,
      elevation: 1,
    },
    md: {
      shadowColor: '#0F1A40',
      shadowOffset: { width: 0, height: 4 },
      shadowOpacity: 0.08,
      shadowRadius: 6,
      elevation: 4,
    },
    lg: {
      shadowColor: '#0F1A40',
      shadowOffset: { width: 0, height: 10 },
      shadowOpacity: 0.08,
      shadowRadius: 15,
      elevation: 8,
    },
    brand: {
      shadowColor: '#F97316',
      shadowOffset: { width: 0, height: 4 },
      shadowOpacity: 0.30,
      shadowRadius: 14,
      elevation: 6,
    },
  },

  // ---------------------------------------------------------------------------
  // Dimensões de componentes
  // ---------------------------------------------------------------------------
  components: {
    buttonHeight: 44,         // touch target mínimo iOS HIG
    buttonHeightSm: 36,
    buttonHeightLg: 52,
    inputHeight: 44,
    inputHeightSm: 36,
    tabBarHeight: 60,
    headerHeight: 56,
    avatarSm: 32,
    avatarMd: 40,
    avatarLg: 56,
    avatarXl: 80,
    iconSm: 16,
    iconMd: 20,
    iconLg: 24,
    iconXl: 32,
  },
} as const;

// ---------------------------------------------------------------------------
// Dark theme — usado com useColorScheme() do React Native
// ---------------------------------------------------------------------------
export const darkTheme = {
  ...theme,
  colors: {
    ...theme.colors,
    primary: '#FD7A38',       // orange-400 — mais claro no dark
    primaryHover: '#F97316',
    primaryLight: 'rgba(249,115,22,0.14)',

    surface: {
      background: '#0C1322',
      card: '#111E35',
      cardPressed: '#172542',
      overlay: 'rgba(0,0,0,0.65)',
      nav: '#070E1D',
      navActive: '#0F1A3A',
    },

    border: '#ffffff17',
    borderStrong: '#ffffff29',
    divider: '#ffffff0D',

    text: {
      primary: 'rgba(255,255,255,0.90)',
      secondary: 'rgba(255,255,255,0.55)',
      tertiary: 'rgba(255,255,255,0.35)',
      disabled: 'rgba(255,255,255,0.22)',
      inverse: '#0C1322',
      onDark: 'rgba(255,255,255,0.90)',
      onDarkSubtle: 'rgba(255,255,255,0.55)',
      link: '#FD7A38',
    },

    success: { light: 'rgba(52,211,153,0.12)', base: '#34D399', dark: '#A7F3D0' },
    warning: { light: 'rgba(251,191,36,0.12)', base: '#FBBF24', dark: '#FDE68A' },
    error:   { light: 'rgba(248,113,113,0.12)', base: '#F87171', dark: '#FECACA' },
    info:    { light: 'rgba(96,165,250,0.12)', base: '#60A5FA', dark: '#BFDBFE' },

    paymentStatus: {
      pending: { bg: 'rgba(251,191,36,0.12)', text: '#FBBF24', border: 'rgba(251,191,36,0.28)', dot: '#FBBF24' },
      paid:    { bg: 'rgba(52,211,153,0.12)', text: '#34D399', border: 'rgba(52,211,153,0.28)', dot: '#34D399' },
      refused: { bg: 'rgba(248,113,113,0.12)', text: '#F87171', border: 'rgba(248,113,113,0.28)', dot: '#F87171' },
    },
  },
  shadows: {
    none:  { shadowColor: 'transparent', shadowOffset: { width: 0, height: 0 }, shadowOpacity: 0, shadowRadius: 0, elevation: 0 },
    sm:    { shadowColor: '#000', shadowOffset: { width: 0, height: 1 }, shadowOpacity: 0.35, shadowRadius: 3, elevation: 2 },
    md:    { shadowColor: '#000', shadowOffset: { width: 0, height: 4 }, shadowOpacity: 0.40, shadowRadius: 8, elevation: 6 },
    lg:    { shadowColor: '#000', shadowOffset: { width: 0, height: 10 }, shadowOpacity: 0.45, shadowRadius: 20, elevation: 12 },
    brand: { shadowColor: '#F97316', shadowOffset: { width: 0, height: 4 }, shadowOpacity: 0.45, shadowRadius: 18, elevation: 8 },
  },
} as const;

/**
 * Hook helper — retorna o tema correto baseado no colorScheme do sistema.
 *
 * Uso:
 *   import { useColorScheme } from 'react-native';
 *   import { useTheme } from '@medcontrol/design-system/native';
 *   const t = useTheme();
 */
export function useTheme() {
  // Importação lazy para não gerar dependência de react-native neste arquivo de tokens
  // eslint-disable-next-line @typescript-eslint/no-require-imports
  const { useColorScheme } = require('react-native') as typeof import('react-native');
  const scheme = useColorScheme();
  return scheme === 'dark' ? darkTheme : theme;
}

export type Theme = typeof theme;
export type DarkTheme = typeof darkTheme;
export type ThemeColors = typeof theme.colors;
export type PaymentStatusKey = keyof typeof theme.colors.paymentStatus;
