/**
 * MM Fature — Color Tokens
 *
 * Brand DNA: Laranja vibrante (M superior) + Navy profundo (texto e M inferior)
 * Modernizado para um visual SaaS limpo, inspirado em dashboards de gestão financeira.
 */

// ---------------------------------------------------------------------------
// Paleta Primária — Laranja MM Fature
// ---------------------------------------------------------------------------
export const orange = {
  50: '#FFF4ED',
  100: '#FFE6D5',
  200: '#FFCBA9',
  300: '#FEA472',
  400: '#FD7A38',
  500: '#F97316', // base brand — referência da logo
  600: '#EA6310',
  700: '#C24E0D',
  800: '#9A3F11',
  900: '#7C3412',
  950: '#431807',
} as const;

// ---------------------------------------------------------------------------
// Paleta Secundária — Navy MM Fature
// ---------------------------------------------------------------------------
export const navy = {
  50: '#EEF2FF',
  100: '#E0E7FF',
  200: '#C7D2FE',
  300: '#A5B4FC',
  400: '#818CF8',
  500: '#6366F1',
  600: '#4F46E5',
  700: '#3730A3',
  800: '#2D2A8A',
  900: '#1B2E63', // base brand — referência da logo
  950: '#0F1A40', // sidebar / superfícies escuras
} as const;

// ---------------------------------------------------------------------------
// Neutros — Escala de cinza quente (leve toque warm)
// ---------------------------------------------------------------------------
export const neutral = {
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
// Semânticas — Feedback visual
// ---------------------------------------------------------------------------
export const semantic = {
  // Sucesso
  success: {
    light: '#ECFDF5',
    base: '#10B981',
    dark: '#065F46',
  },
  // Atenção
  warning: {
    light: '#FFFBEB',
    base: '#F59E0B',
    dark: '#92400E',
  },
  // Erro
  error: {
    light: '#FEF2F2',
    base: '#EF4444',
    dark: '#991B1B',
  },
  // Informação
  info: {
    light: '#EFF6FF',
    base: '#3B82F6',
    dark: '#1E40AF',
  },
} as const;

// ---------------------------------------------------------------------------
// Tokens de Superfície — mapeamento semântico do layout
// ---------------------------------------------------------------------------
export const surface = {
  // Fundos
  background: neutral[50],       // página principal
  card: neutral[0],              // cards / panels
  cardHover: neutral[100],       // hover state de card
  overlay: 'rgba(15, 26, 64, 0.48)', // modal backdrop

  // Sidebar / navegação escura
  sidebar: navy[950],
  sidebarActive: navy[900],
  sidebarHover: 'rgba(255, 255, 255, 0.06)',

  // Bordas
  border: neutral[200],
  borderStrong: neutral[300],
  divider: neutral[100],
} as const;

// ---------------------------------------------------------------------------
// Tokens de Texto
// ---------------------------------------------------------------------------
export const text = {
  primary: neutral[900],
  secondary: neutral[600],
  tertiary: neutral[500],
  disabled: neutral[400],
  inverse: neutral[0],
  onDark: 'rgba(255, 255, 255, 0.87)',
  onDarkSubtle: 'rgba(255, 255, 255, 0.55)',
  link: orange[600],
  linkHover: orange[700],
} as const;

// ---------------------------------------------------------------------------
// Tokens de Ação — botões e interativos
// ---------------------------------------------------------------------------
export const action = {
  primary: orange[500],
  primaryHover: orange[600],
  primaryActive: orange[700],
  primaryText: neutral[0],

  secondary: navy[900],
  secondaryHover: navy[950],
  secondaryActive: navy[950],
  secondaryText: neutral[0],

  ghost: 'transparent',
  ghostHover: neutral[100],
  ghostActive: neutral[200],
  ghostText: neutral[800],

  danger: semantic.error.base,
  dangerHover: semantic.error.dark,
  dangerText: neutral[0],
} as const;

// ---------------------------------------------------------------------------
// Status de Pagamento (domínio Payment)
// ---------------------------------------------------------------------------
export const paymentStatus = {
  pending: {
    bg: '#FFFBEB',
    text: '#92400E',
    border: '#FDE68A',
    dot: semantic.warning.base,
  },
  paid: {
    bg: '#ECFDF5',
    text: '#065F46',
    border: '#A7F3D0',
    dot: semantic.success.base,
  },
  refused: {
    bg: '#FEF2F2',
    text: '#991B1B',
    border: '#FECACA',
    dot: semantic.error.base,
  },
} as const;

// ---------------------------------------------------------------------------
// Dark Mode — tokens semânticos sobrescritos
// Uso: aplica quando data-theme="dark" ou prefers-color-scheme: dark
// A paleta base (orange, navy, neutral) não muda — apenas os mapeamentos.
// ---------------------------------------------------------------------------
export const dark = {
  surface: {
    background: '#0C1322',
    card: '#111E35',
    cardHover: '#172542',
    overlay: 'rgba(0, 0, 0, 0.65)',
    nav: '#070E1D',
    navActive: '#0F1A3A',
  },
  text: {
    primary: 'rgba(255, 255, 255, 0.90)',
    secondary: 'rgba(255, 255, 255, 0.55)',
    tertiary: 'rgba(255, 255, 255, 0.35)',
    disabled: 'rgba(255, 255, 255, 0.22)',
    link: orange[400],
    linkHover: orange[300],
  },
  border: {
    default: 'rgba(255, 255, 255, 0.09)',
    strong: 'rgba(255, 255, 255, 0.16)',
    divider: 'rgba(255, 255, 255, 0.05)',
  },
  action: {
    primary: orange[400],
    primaryHover: orange[500],
    secondary: navy[700],
    secondaryHover: navy[600],
    danger: '#F87171',
  },
  semantic: {
    success: { light: 'rgba(52,211,153,0.12)', base: '#34D399', dark: '#A7F3D0' },
    warning: { light: 'rgba(251,191,36,0.12)', base: '#FBBF24', dark: '#FDE68A' },
    error:   { light: 'rgba(248,113,113,0.12)', base: '#F87171', dark: '#FECACA' },
    info:    { light: 'rgba(96,165,250,0.12)', base: '#60A5FA', dark: '#BFDBFE' },
  },
  paymentStatus: {
    pending: { bg: 'rgba(251,191,36,0.12)', text: '#FBBF24', border: 'rgba(251,191,36,0.28)', dot: '#FBBF24' },
    paid:    { bg: 'rgba(52,211,153,0.12)', text: '#34D399', border: 'rgba(52,211,153,0.28)', dot: '#34D399' },
    refused: { bg: 'rgba(248,113,113,0.12)', text: '#F87171', border: 'rgba(248,113,113,0.28)', dot: '#F87171' },
  },
} as const;

// ---------------------------------------------------------------------------
// Export consolidado
// ---------------------------------------------------------------------------
export const colors = {
  orange,
  navy,
  neutral,
  semantic,
  surface,
  text,
  action,
  paymentStatus,
  dark,
} as const;
