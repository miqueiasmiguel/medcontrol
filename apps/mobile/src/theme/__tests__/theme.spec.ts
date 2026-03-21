import { colors, spacing, typography, theme } from '../index';

describe('Design System', () => {
  describe('colors', () => {
    it('tem as cores obrigatórias do MedControl', () => {
      expect(colors.primary).toBe('#0EA5E9');
      expect(colors.primaryDark).toBe('#0C4A6E');
      expect(colors.error).toBe('#EF4444');
      expect(colors.background).toBe('#FFFFFF');
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
