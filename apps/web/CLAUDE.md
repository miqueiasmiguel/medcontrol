# MedControl Web

## Stack

- Angular 20.3 | Standalone components | Signals | OnPush everywhere
- Angular Material 20.2 (minimal: spinner + snackbar)
- TypeScript strict mode
- Jest (via Nx) | 75% coverage threshold

## Estrutura

```
src/app/
├── app.ts                    ← root component (apenas <router-outlet>)
├── app.routes.ts             ← rotas lazy, redirect / → /auth/login
├── app.config.ts             ← provideRouter, provideHttpClient, provideAnimationsAsync
├── core/
│   └── tokens/
│       └── window.token.ts  ← InjectionToken<Window> para mockabilidade em testes
└── auth/
    ├── auth.routes.ts        ← lazy routes: login, magic-link-sent, callback
    ├── data-access/
    │   ├── auth.service.ts   ← sendMagicLink, verifyMagicLink, loginWithGoogle, logout
    │   └── session.service.ts ← isAuthenticated() lê cookie mmc_session
    ├── guards/
    │   ├── auth.guard.ts     ← redireciona para /auth/login se não autenticado
    │   └── guest.guard.ts    ← redireciona para / se já autenticado
    ├── interceptors/
    │   └── auth.interceptor.ts ← withCredentials: true em /api/*
    ├── login/                ← LoginComponent (magic link form + Google button)
    ├── magic-link-sent/      ← MagicLinkSentComponent (confirmação)
    └── google-callback/      ← GoogleCallbackComponent (troca code → cookie)
```

## Autenticação com HttpOnly Cookies

O Angular **não acessa os tokens diretamente**. O backend seta cookies HttpOnly.

- `mmc_access_token` / `mmc_refresh_token`: HttpOnly, nunca visíveis via JS
- `mmc_session=1`: não HttpOnly — lido por `SessionService.isAuthenticated()`

O interceptor garante `withCredentials: true` em todos os requests para `/api/*`, fazendo o browser enviar os cookies automaticamente.

## Proxy de Desenvolvimento

`proxy.conf.json` mapeia `/api → http://localhost:5000`. Isso garante que `SameSite=Strict` funcione em dev (mesma origem aparente para o browser).

## Convenções

```typescript
// ✅ Standalone + OnPush
@Component({ standalone: true, changeDetection: ChangeDetectionStrategy.OnPush })

// ✅ Signals em vez de BehaviorSubject
readonly loading = signal(false);
readonly errorMessage = signal('');

// ✅ takeUntilDestroyed com DestroyRef injetado
private readonly destroyRef = inject(DestroyRef);
observable$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(...)

// ✅ inject() em vez de construtor
private readonly auth = inject(AuthService);

// ✅ WINDOW token para mockabilidade
private readonly win = inject(WINDOW);
this.win.location.href = '...'; // não usar window.location diretamente

// ❌ Proibido
any | subscribe sem unsubscribe | public setters | lógica em templates complexa
```

## Testes

- `SessionService`: sem TestBed (`new SessionService()`)
- Serviços HTTP: `HttpTestingController`
- Componentes: `TestBed.configureTestingModule`, `fakeAsync/tick`
- Guards: `TestBed.runInInjectionContext` + jest mocks
- Spy em `Router.navigate` via `jest.spyOn` no `beforeEach` quando o componente navega

## Comandos

```bash
# Da raiz do monorepo
pnpm nx test web --coverage       # testes + cobertura
pnpm nx lint web
pnpm nx run web:type-check
pnpm nx serve web                 # http://localhost:4200
```
