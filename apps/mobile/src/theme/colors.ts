export const colors = {
  primary: '#0EA5E9',
  primaryDark: '#0C4A6E',
  primaryLight: '#E0F2FE',
  background: '#FFFFFF',
  surface: '#F8FAFC',
  error: '#EF4444',
  textPrimary: '#0F172A',
  textSecondary: '#64748B',
  border: '#E2E8F0',
  white: '#FFFFFF',
  black: '#000000',
} as const;

export type ColorToken = keyof typeof colors;
