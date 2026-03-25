export const colors = {
  // Brand primary — laranja MM Fature
  primary: '#F97316',
  primaryDark: '#EA6310',
  primaryLight: '#FFF4ED',

  // Brand secondary — navy MM Fature
  navy: '#1B2E63',
  navyDark: '#0F1A40',

  // Superfícies
  background: '#F8F9FA',
  surface: '#FFFFFF',

  // Status semânticos
  error: '#EF4444',
  success: '#10B981',
  warning: '#F59E0B',

  // Texto
  textPrimary: '#212529',
  textSecondary: '#868E96',
  textDisabled: '#CED4DA',

  // Bordas
  border: '#E9ECEF',
  borderStrong: '#DEE2E6',

  // Utilitários
  white: '#FFFFFF',
  black: '#000000',
} as const;

export type ColorToken = keyof typeof colors;
