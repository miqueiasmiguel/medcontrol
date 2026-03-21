# MedControl Mobile — Guia para o Claude

## Stack

| Tecnologia | Versão | Uso |
|---|---|---|
| React Native (Expo) | SDK 55 | Framework |
| TypeScript | ~5.9.2 | Linguagem (strict mode) |
| Expo Router | ^4.0 | Navegação file-based |
| React Native Paper | ^5.15 | Componentes UI (MD3) |
| react-hook-form | ^7 | Formulários |
| expo-auth-session | ^6 | Google OAuth |
| expo-web-browser | ^14 | Abre browser para OAuth |
| expo-linking | ^7 | Deep links |
| @react-native-async-storage/async-storage | ^2 | Sessão persistida |
| zod | ^3 | Validação de esquemas |

## Estrutura de Arquivos

```
apps/mobile/
├── app/                         # Expo Router (file-based routing)
│   ├── _layout.tsx              # Root: PaperProvider + SafeAreaProvider
│   ├── index.tsx                # Redirect: auth? → /(app) : /(auth)/login
│   ├── (auth)/                  # Grupo de autenticação (não autenticado)
│   │   ├── _layout.tsx          # Stack sem header
│   │   ├── login.tsx            # → LoginScreen
│   │   ├── magic-link-sent.tsx  # → MagicLinkSentScreen
│   │   └── verify.tsx           # → MagicLinkVerifyScreen (deep link)
│   └── (app)/                   # Grupo protegido (requer autenticação)
│       ├── _layout.tsx          # Redireciona para /login se não auth
│       └── index.tsx            # Home placeholder
├── src/
│   ├── theme/                   # Design system
│   │   ├── colors.ts            # Tokens de cor (primary: #0EA5E9)
│   │   ├── spacing.ts           # Tokens de espaçamento (xs→xxl)
│   │   ├── typography.ts        # Tamanhos e pesos de fonte
│   │   └── index.ts             # Exporta tudo + tema React Native Paper
│   ├── services/
│   │   └── auth.service.ts      # HTTP: sendMagicLink, verifyMagicLink, loginWithGoogle, logout
│   ├── hooks/
│   │   └── useAuth.ts           # Estado de sessão (AsyncStorage key: mmc_session)
│   └── components/ui/
│       ├── AppButton.tsx        # Botão com variante filled/outline + loading
│       └── AppTextInput.tsx     # Input com label Paper + mensagem de erro
└── src/screens/auth/
    ├── LoginScreen.tsx          # Email + Google OAuth (expo-auth-session)
    ├── MagicLinkSentScreen.tsx  # Confirmação de envio (recebe email via params)
    └── MagicLinkVerifyScreen.tsx # Verificação do token (deep link: medcontrol://verify?token=xxx)
```

## Autenticação Mobile

### Fluxo Magic Link

1. Usuário digita email → `AuthService.sendMagicLink(email)`
2. Backend envia email com link `medcontrol://verify?token=xxx`
3. Usuário toca no link → app abre via deep link → `app/(auth)/verify.tsx`
4. `MagicLinkVerifyScreen` chama `AuthService.verifyMagicLink(token)`
5. Backend valida token, seta cookies HttpOnly → app navega para `/(app)`

### Fluxo Google OAuth

1. `LoginScreen` usa `Google.useAuthRequest` (expo-auth-session) com `redirectUri: medcontrol://google-callback`
2. `promptAsync()` abre browser → usuário consente
3. `response.type === 'success'` → `AuthService.loginWithGoogle(code, redirectUri)`
4. Backend troca code por tokens, retorna `AuthTokenDto` no body → app navega para `/(app)`

### Sessão

- Backend seta cookies HttpOnly (gerenciados pelo native HTTP stack automaticamente)
- Flag de sessão salva em AsyncStorage (`mmc_session=1`) como proxy de estado UI
- `useAuth` hook lê/escreve essa flag

## Deep Linking

Scheme configurado: `medcontrol://`

| URL | Destino |
|---|---|
| `medcontrol://verify?token=xxx` | `app/(auth)/verify.tsx` |
| `medcontrol://google-callback` | Capturado pelo expo-auth-session internamente |

## Testes

```bash
# De apps/mobile/
pnpm test                  # todos os testes
pnpm test:watch            # modo watch
pnpm test:coverage         # com cobertura (threshold: 70%)

# Arquivo específico
pnpm test src/services/__tests__/auth.service.spec.ts
```

### Setup de Testes

- **Preset**: `jest-expo` v55
- **Biblioteca**: `@testing-library/react-native` v12
- **Config**: `jest.config.js` na raiz do mobile
- **Setup**: `jest.setup.ts` (estende jest-native matchers)
- **Wrapper**: envolver com `<PaperProvider>` em testes de componentes UI

### Padrões de Mock

```typescript
// Mock de AsyncStorage
jest.mock('@react-native-async-storage/async-storage', () =>
  require('@react-native-async-storage/async-storage/jest/async-storage-mock'),
);

// Mock de fetch
const mockFetch = jest.fn();
global.fetch = mockFetch;

// Mock de expo-router
jest.mock('expo-router', () => ({
  useRouter: () => ({ push: jest.fn(), replace: jest.fn(), back: jest.fn() }),
  useLocalSearchParams: () => ({ token: 'test-token' }),
}));

// Mock de expo-auth-session
jest.mock('expo-auth-session', () => ({
  makeRedirectUri: () => 'medcontrol://google-callback',
}));
jest.mock('expo-auth-session/providers/google', () => ({
  useAuthRequest: () => [null, null, jest.fn()],
}));
```

## Comandos

```bash
# Iniciar (de apps/mobile/)
pnpm start            # expo start
pnpm android          # expo start --android
pnpm ios              # expo start --ios
pnpm web              # expo start --web

# Via NX (da raiz)
pnpm nx run mobile:start
pnpm nx run mobile:android
```

## Variáveis de Ambiente (app.json extra)

| Chave | Descrição |
|---|---|
| `apiUrl` | URL do backend (dev: `http://localhost:5000`) |
| `googleClientId` | Client ID do Google OAuth |

Configurar em `app.json > expo > extra` ou via `app.config.ts` para produção.

## O que ainda não foi implementado

- Telas de pagamentos (módulo principal)
- Perfil do médico
- Relatórios
- Notificações push
- Configurações do usuário
