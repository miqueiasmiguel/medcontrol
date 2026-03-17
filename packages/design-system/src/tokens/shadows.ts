/**
 * MM Fature — Shadow Tokens
 *
 * Sombras sutis com leve toque azulado (coerente com o navy da marca)
 */

export const shadows = {
  none: 'none',

  // Elevação baixa — cards em repouso
  sm: '0px 1px 2px 0px rgba(15, 26, 64, 0.05)',

  // Elevação média — cards hover, dropdowns
  md: '0px 4px 6px -1px rgba(15, 26, 64, 0.08), 0px 2px 4px -1px rgba(15, 26, 64, 0.04)',

  // Elevação alta — modais, popovers
  lg: '0px 10px 15px -3px rgba(15, 26, 64, 0.08), 0px 4px 6px -2px rgba(15, 26, 64, 0.04)',

  // Elevação muito alta — toasts, dialogs
  xl: '0px 20px 25px -5px rgba(15, 26, 64, 0.10), 0px 10px 10px -5px rgba(15, 26, 64, 0.04)',

  // Destaque laranja — hover em botão primary, card selecionado
  brand: '0px 4px 14px 0px rgba(249, 115, 22, 0.30)',

  // Inner shadow — input focus, campos
  inner: 'inset 0px 2px 4px 0px rgba(15, 26, 64, 0.06)',

  // Focus ring — acessibilidade
  focusRing: '0px 0px 0px 3px rgba(249, 115, 22, 0.25)',
  focusRingDark: '0px 0px 0px 3px rgba(249, 115, 22, 0.40)',
} as const;
