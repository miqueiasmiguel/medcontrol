/**
 * MM Fature — Spacing Tokens
 *
 * Base grid: 4px
 * Nomenclatura numérica onde N = N × 4px
 */

export const spacing = {
  0: '0px',
  0.5: '2px',
  1: '4px',
  1.5: '6px',
  2: '8px',
  2.5: '10px',
  3: '12px',
  3.5: '14px',
  4: '16px',
  5: '20px',
  6: '24px',
  7: '28px',
  8: '32px',
  9: '36px',
  10: '40px',
  12: '48px',
  14: '56px',
  16: '64px',
  20: '80px',
  24: '96px',
  28: '112px',
  32: '128px',
  36: '144px',
  40: '160px',
  48: '192px',
  56: '224px',
  64: '256px',
} as const;

// ---------------------------------------------------------------------------
// Layout tokens — dimensões fixas de componentes
// ---------------------------------------------------------------------------
export const layout = {
  // Sidebar
  sidebarWidth: '240px',
  sidebarCollapsedWidth: '64px',

  // Topbar
  topbarHeight: '64px',

  // Conteúdo
  contentMaxWidth: '1440px',
  contentPaddingX: spacing[6],
  contentPaddingY: spacing[6],

  // Cards
  cardPadding: spacing[6],
  cardPaddingSm: spacing[4],

  // Formulários
  inputHeight: '40px',
  inputHeightSm: '32px',
  inputHeightLg: '48px',
} as const;

export type SpacingKey = keyof typeof spacing;
