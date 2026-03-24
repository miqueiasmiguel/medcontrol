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
│       ├── __tests__/
│       │   └── index.spec.tsx   # Testes do HomeScreen (logout)
│       └── index.tsx            # HomeScreen (pagamentos + botão de logout)
├── src/
│   ├── theme/                   # Design system
│   │   ├── colors.ts            # Tokens de cor (primary: #0EA5E9)
│   │   ├── spacing.ts           # Tokens de espaçamento (xs→xxl)
│   │   ├── typography.ts        # Tamanhos e pesos de fonte
│   │   └── index.ts             # Exporta tudo + tema React Native Paper
│   ├── services/
│   │   ├── auth.service.ts      # HTTP: sendMagicLink, verifyMagicLink, loginWithGoogle, verifyGoogleIdToken, logout
│   │   ├── payment.service.ts   # HTTP: listPayments (GET /payments) — PaymentDto, PaymentStatus, ListPaymentsParams
│   │   └── health-plan.service.ts # HTTP: listHealthPlans (GET /health-plans) — HealthPlanDto
│   ├── hooks/
│   │   ├── useAuth.ts           # Estado de sessão (AsyncStorage key: mmc_session)
│   │   ├── usePayments.ts       # Carrega pagamentos do backend; retorna { payments, loading, error, refetch }
│   │   └── useCurrentUser.ts    # Carrega dados do usuário autenticado; retorna { user, loading, error }
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

### Fluxo Google OAuth (Mobile — authorization code + PKCE)

1. `LoginScreen` usa `Google.useAuthRequest` com `androidClientId`, `responseType: ResponseType.Code`, `usePKCE: true`
2. Antes de chamar `promptAsync()`, salva `request.codeVerifier` e `redirectUri` no AsyncStorage
3. `promptAsync()` abre browser → usuário consente
4. Google retorna `code` como query param no redirect URI nativo (`<reverse-client-id>:/oauth2redirect/google`)
5. Expo Router roteia para `app/oauth2redirect/google.tsx`, que lê `code` via `useLocalSearchParams`
6. `google.tsx` lê `codeVerifier` e `redirectUri` do AsyncStorage e troca o `code` por tokens em `https://oauth2.googleapis.com/token` (sem `client_secret` — Android clients não precisam)
7. `AuthService.verifyGoogleIdToken(id_token)` → `POST /auth/google/verify` no backend
8. Backend verifica via `GET https://oauth2.googleapis.com/tokeninfo?id_token=...` (sem `client_secret`)
9. Backend cria/atualiza usuário, seta cookies HttpOnly → app navega para `/(app)`

> **Por que `ResponseType.Code` e não `IdToken`**: Android OAuth clients do Google Cloud Console **não suportam `response_type=id_token`** (fluxo implícito). Apenas `response_type=code` é aceito. A troca do code é feita no próprio app (sem secret) porque Android clients são clientes públicos.

> **Separação web vs mobile**: o web Angular usa `response_type=code` com `webClientId` + exchange server-side (`POST /auth/google/callback`). O mobile usa `response_type=code` com PKCE + troca client-side + verificação do `id_token` (`POST /auth/google/verify`). Os dois fluxos coexistem no backend.

### Sessão

- Backend seta cookies HttpOnly (gerenciados pelo native HTTP stack automaticamente)
- Flag de sessão salva em AsyncStorage (`mmc_session=1`) como proxy de estado UI
- `useAuth` hook lê/escreve essa flag

## Deep Linking

Scheme configurado: `medcontrol://`

| URL | Destino |
|---|---|
| `medcontrol://verify?token=xxx` | `app/(auth)/verify.tsx` |
| `<reverse-client-id>:/oauth2redirect/google?id_token=...` | `app/oauth2redirect/google.tsx` |

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

// Mock de expo-auth-session (incluir ResponseType para LoginScreen)
jest.mock('expo-auth-session', () => ({
  makeRedirectUri: () => 'com.googleusercontent.apps.XXX:/oauth2redirect/google',
  ResponseType: { IdToken: 'id_token', Code: 'code' },
}));
jest.mock('expo-auth-session/providers/google', () => ({
  useAuthRequest: () => [{ codeVerifier: 'test-verifier' }, null, jest.fn()],
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

## Armadilhas Conhecidas

### Google OAuth — handler duplicado causa race condition no Android físico
- **Problema**: No Android com dev build, quando o callback OAuth chega, **dois handlers disparam simultaneamente**: o `useEffect` que observa `googleResponse` em `LoginScreen` (via expo-auth-session) E o `google.tsx` (via Expo Router). O primeiro consome o código (uso único) com sucesso e navega para `/(app)`; o segundo falha com código já consumido e redireciona silenciosamente para `/(auth)/login`, sobrescrevendo a navegação.
- **Correto**: `LoginScreen` **não deve chamar `loginWithGoogle` diretamente**. O único handler é `app/oauth2redirect/google.tsx`, roteado pelo Expo Router. `LoginScreen` usa `useAuthRequest` apenas para chamar `promptAsync()`.

### Google OAuth — erros de autenticação devem ser visíveis ao usuário
- **Problema**: O catch em `google.tsx` redirecionava para login sem passar o erro, deixando o usuário sem feedback.
- **Correto**: Usar `router.replace({ pathname: '/(auth)/login', params: { error: msg } })` e exibir `oauthError` via `useLocalSearchParams` em `LoginScreen`.

### Google OAuth — mobile usa authorization code + PKCE, não id_token implícito
- **Problema**: Android OAuth clients do Google não suportam `response_type=id_token` (fluxo implícito). Tentativas de usar `ResponseType.IdToken` com `androidClientId` resultam em `400 unsupported_response_type` do Google.
- **Correto**: Mobile usa `responseType: ResponseType.Code` com `usePKCE: true` e `androidClientId`. Antes de `promptAsync()`, salvar `request.codeVerifier` e `redirectUri` no AsyncStorage. Em `google.tsx`, trocar o `code` por tokens em `https://oauth2.googleapis.com/token` (sem `client_secret` — Android clients são públicos) e extrair o `id_token` da resposta para enviar ao backend via `POST /auth/google/verify`.

### IP do backend no app.json precisa refletir o IP real da máquina
- O campo `extra.apiUrl` em `apps/mobile/app.json` deve usar o IP local da máquina de desenvolvimento (ex: `http://192.168.0.xxx:5113`). `localhost` não funciona em dispositivos físicos. Atualizar sempre que o IP mudar (DHCP).

## Serviços HTTP

Além do `AuthService`, existem:

| Serviço | Arquivo | Endpoints |
|---|---|---|
| `PaymentService` | `src/services/payment.service.ts` | `GET /payments` com `ListPaymentsParams` opcionais |
| `HealthPlanService` | `src/services/health-plan.service.ts` | `GET /health-plans` |
| `UserService` | `src/services/user.service.ts` | `GET /users/me` — retorna `UserDto` |

### Tipos compartilhados

`payment.service.ts` exporta:
- `PaymentStatus` — `'Pending' | 'Paid' | 'Refused' | 'PartiallyPending' | 'PartiallyRefused'`
- `PaymentItemDto` — `{ id, procedureId, value, status, notes? }`
- `PaymentDto` — campos completos + `totalValue` (soma dos itens) + `items`

### Hook `usePayments`

`src/hooks/usePayments.ts` — carrega pagamentos do backend via `PaymentService.listPayments()`. Retorna `{ payments, loading, error, refetch }`.

## O que ainda não foi implementado

- Relatórios
- Notificações push
