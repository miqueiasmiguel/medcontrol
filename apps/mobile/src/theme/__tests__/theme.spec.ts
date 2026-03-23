import { colors, spacing, typography, theme } from '../index';

describe('Design System', () => {
  describe('colors', () => {
    it('tem as cores obrigatórias do MedControl', () => {
      // Laranja — brand primary
      expect(colors.primary).toBe('#F97316');
      expect(colors.primaryDark).toBe('#EA6310');
      // Navy — brand secondary
      expect(colors.navy).toBe('#1B2E63');
      expect(colors.navyDark).toBe('#0F1A40');
      // Semânticas
      expect(colors.error).toBe('#EF4444');
      // Superfícies
      expect(colors.background).toBe('#F8F9FA');
      expect(colors.surface).toBe('#FFFFFF');
    });
  });

  describe('spacing', () => {
    it('tem todos os tokens de espaçamento', () => {
      expect(spacing.xs).toBe(4);
      expect(spacing.sm).toBe(8);
      expect(spacing.md).toBe(16);
      expect(spacing.lg).toBe(24);
      expect(spacing.xl).toBe(32);
      expect(spacing.xxl).toBe(48);
    });
  });

  describe('typography', () => {
    it('tem tamanhos de fonte definidos', () => {
      expect(typography.sizes.sm).toBe(14);
      expect(typography.sizes.md).toBe(16);
    });

    it('tem pesos de fonte definidos', () => {
      expect(typography.weights.regular).toBe('400');
      expect(typography.weights.bold).toBe('700');
    });
  });

  describe('theme (React Native Paper)', () => {
    it('estende MD3LightTheme', () => {
      expect(theme.dark).toBe(false);
    });

    it('usa a cor primária do MedControl', () => {
      expect(theme.colors.primary).toBe(colors.primary);
    });

    it('usa a cor de erro do MedControl', () => {
      expect(theme.colors.error).toBe(colors.error);
    });
  });
});
