# MedControl Mobile — Guia para o Claude

## Stack

React Native (Expo SDK 54) + TypeScript strict, testado com Jest + `@testing-library/react-native`.

## Estrutura de Pastas

```
src/
├── app/
│   └── App.tsx                   # Entry point — monta RootNavigator
├── navigation/
│   ├── types.ts                  # AuthStackParamList (Login | MagicLinkSent)
│   └── RootNavigator.tsx         # NavigationContainer + Stack
├── screens/
│   └── auth/
│       ├── LoginScreen.tsx       # Email input → magic link ou Google OAuth
│       └── MagicLinkSentScreen.tsx
├── components/
│   └── ui/
│       ├── Button/               # variant: primary | ghost; props: loading, disabled
│       └── TextInput/            # label + error inline
└── theme/
    ├── colors.ts                 # Paleta orange/navy/neutral + semantic
    ├── typography.ts             # fontSizes, fontWeights, lineHeights
    ├── spacing.ts                # Múltiplos de 4px + radius
    └── index.ts                  # Re-export
```

## Navegação

- Stack: `@react-navigation/native` + `@react-navigation/native-stack`
- `headerShown: false` em todas as telas
- Tipos em `src/navigation/types.ts` (`AuthStackParamList`)

## Design System

Tokens em `src/theme/` espelham o design system web (`packages/design-system/src/web/`):

| Token | Valor |
|---|---|
| Primária (orange-500) | `#F97316` |
| Secundária (navy-900) | `#1B2E63` |
| Erro | `#EF4444` |
| Background card | `#FFFFFF` |
| Background página | `#F3F4F6` (neutral-100) |

## Convenções de Teste

```tsx
// Mock de navigation prop em testes de Screen
const mockNavigation = { navigate: jest.fn(), goBack: jest.fn(), ... };
const mockRoute = { key: 'Login', name: 'Login', params: undefined };
render(<LoginScreen navigation={mockNavigation as any} route={mockRoute as any} />);

// Mock de RootNavigator em App.spec — não usa NavigationContainer real
jest.mock('../navigation/RootNavigator', () => ({
  RootNavigator: () => { /* renderiza LoginScreen diretamente */ },
}));
```

- `react-native-safe-area-context` e `react-native-screens` são mockados em `src/test-setup.ts`
- `RootNavigator` tem cobertura 0% intencionalmente — não é testado por unit tests (depende de engine nativo)
- Cobertura mínima: **70%**

## Screens implementadas

| Screen | Rota | Descrição |
|---|---|---|
| LoginScreen | `Login` | Email input + "Enviar link" + Google OAuth (stub) |
| MagicLinkSentScreen | `MagicLinkSent` | Confirmação de envio, email em pill, botão retry |

## O que ainda não foi implementado

- Serviço de autenticação (`AuthService`) — `handleMagicLink` navega diretamente sem chamar API
- Google OAuth (`handleGoogle` é stub vazio)
- Tela autenticada (home/dashboard do médico)
- Interceptor HTTP / token storage (`expo-secure-store`)
- Deep link para receber o magic link token
