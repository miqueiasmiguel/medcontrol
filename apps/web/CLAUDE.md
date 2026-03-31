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
├── app.routes.ts             ← rotas lazy; rota raiz usa ShellComponent com children
├── app.config.ts             ← provideRouter, provideHttpClient, provideAnimationsAsync
├── core/
│   ├── tokens/
│   │   └── window.token.ts  ← InjectionToken<Window> para mockabilidade em testes
│   ├── data-access/
│   │   └── current-user.service.ts ← signals: #user, currentUser, isDoctor, isGlobalAdmin (computed: globalRole === 'Admin')
│   └── guards/
│       ├── doctor-onboarding.guard.ts ← CanActivateFn; skip se mmc_onboarding_skip no sessionStorage;
│       │                                  getMe() → tenantRole=doctor → getDoctorProfile() → null → redirect /onboarding
│       │                                  aplicado como canActivateChild no shell route (app.routes.ts)
│       └── global-admin.guard.ts ← CanActivateFn; getMe() → globalRole=Admin → true; outros → redirect /
├── admin/
│   ├── admin.routes.ts       ← AdminShellComponent + children: AdminTenantsComponent
│   ├── admin-shell/          ← AdminShellComponent: layout simples (sem sidebar de tenant)
│   ├── admin-tenants/        ← AdminTenantsComponent: tabela de tenants com toggle Ativar/Desativar
│   └── data-access/
│       └── admin-tenants.service.ts ← listTenants() → GET /api/admin/tenants
│                                        setTenantStatus(id, isActive) → PATCH /api/admin/tenants/{id}/status
├── layout/
│   ├── shell/
│   │   └── shell.component.ts   ← ShellComponent: sidebar + <router-outlet>; signal collapsed
│   └── sidebar/
│       └── sidebar.component.ts ← SidebarComponent: nav groups + logout; @Input collapsed
├── auth/
│   ├── auth.routes.ts        ← lazy routes: login, magic-link-sent, callback, verify
│   ├── data-access/
│   │   ├── auth.service.ts   ← sendMagicLink, verifyMagicLink, loginWithGoogle, logout
│   │   └── session.service.ts ← isAuthenticated() lê cookie mmc_session
│   ├── guards/
│   │   ├── auth.guard.ts     ← redireciona para /auth/login se não autenticado
│   │   └── guest.guard.ts    ← redireciona para / se já autenticado
│   ├── interceptors/
│   │   └── auth.interceptor.ts ← withCredentials: true em /api/*
│   ├── login/                ← LoginComponent (magic link form + Google button)
│   ├── magic-link-sent/      ← MagicLinkSentComponent (confirmação)
│   ├── magic-link-callback/  ← MagicLinkCallbackComponent: trampoline (tenta medcontrol://verify primeiro, 2500ms fallback web)
│   └── google-callback/      ← GoogleCallbackComponent (troca code → cookie)
├── doctors/
│   ├── doctors.routes.ts     ← { path: '' → DoctorsListComponent }
│   ├── data-access/
│   │   └── doctor.service.ts ← getDoctors, createDoctor(inviteEmail?), updateDoctor, linkDoctorToUser,
│   │                            inviteAndLinkMember, getMyDoctorProfile, createMyDoctorProfile
│   ├── doctors-list/         ← tabela de médicos + botão "Novo médico"; signals: doctors, formOpen, selectedDoctor,
│   │                            linkFormOpen, doctorToLink, showLinkHint
│   │                            empty state: dois cards explicativos (fluxo 1A e 1B)
│   │                            banner 8s pós-criação sem convite (showLinkHint); onCreatedWithoutInvite()
│   ├── doctor-form/          ← slide-over panel; @Input doctor (null=criar); @Output saved/closed/createdWithoutInvite
│   │                            inviteCheckbox + inviteEmail (create mode only); effect() ajusta validators
│   ├── doctor-link-form/     ← slide-over para vincular doctor a membro; cards radio com avatar+initials
│   │                            linkMode signal: 'existing'|'invite'; modo invite: inviteEmail field
│   │                            empty state: botão "Convidar e vincular por e-mail" (switch-to-invite)
│   │                            AvailableMember: { userId, label, email, initials }; imports RouterLink + MatProgressSpinnerModule
│   └── onboarding/           ← DoctorOnboardingComponent: layout full-page (sem sidebar), form name/crm/councilState/specialty
│                                submit → POST /users/me/doctor-profile → navega /doctors
│                                "Fazer depois" → sessionStorage.setItem('mmc_onboarding_skip','1') → /doctors
├── health-plans/
│   ├── health-plans.routes.ts ← { path: '' → HealthPlansListComponent }
│   ├── data-access/
│   │   └── health-plan.service.ts ← getHealthPlans, createHealthPlan, updateHealthPlan; HealthPlanDto
│   ├── health-plans-list/    ← tabela de convênios + botão "Novo convênio"; signals: healthPlans, formOpen, selectedHealthPlan
│   └── health-plan-form/     ← slide-over panel; @Input healthPlan (null=criar); @Output saved/closed
├── procedures/
│   ├── procedures.routes.ts  ← { path: '' → ProceduresListComponent }
│   ├── data-access/
│   │   └── procedure.service.ts ← getProcedures, createProcedure, updateProcedure; ProcedureDto
│   ├── procedures-list/      ← tabela de procedimentos + botão "Novo procedimento"; signals: procedures, formOpen, selectedProcedure; CurrencyPipe para valor
│   └── procedure-form/       ← slide-over panel; @Input procedure (null=criar); @Output saved/closed; value type=number
├── payments/
│   ├── payments.routes.ts    ← { path: '' → PaymentsListComponent }
│   ├── data-access/
│   │   └── payment.service.ts ← getPayments, getPayment, createPayment, updatePayment, updatePaymentItemStatus, addPaymentItem, removePaymentItem
│   │                             PaymentDto (com status computado), PaymentItemDto, PaymentStatus, PaymentItemStatus
│   ├── payments-list/        ← tabela de pagamentos + filtro por status (todos os 5 valores); enriched com doctor/healthPlan names; uses payment.status direto do dto
│   ├── payment-form/         ← slide-over (600px) para criar pagamento; FormArray de itens com procedureId+value; auto-fill value ao selecionar procedimento
│   └── payment-detail/       ← slide-over (560px) para ver/editar cabeçalho, atualizar status de itens, adicionar e remover itens
├── members/
│   ├── members.routes.ts     ← { path: '' → MembersComponent }
│   ├── data-access/
│   │   └── members.service.ts ← getMembers, addMember, updateMemberRole, removeMember; MemberDto
│   ├── member-form/          ← slide-over panel; @Input member (null=add, set=edit role only); @Output saved/closed
│   │                            add mode: email + role; edit mode: role only (email disabled)
│   │                            roleOptions: admin/operator/doctor (sem owner via UI)
│   └── members.component.ts  ← tabela de membros; signals: members, loading, formOpen, selectedMember
│                                getRoleLabel(role) → mapa: admin/operator/doctor/owner → labels PT-BR
└── tenants/
    ├── tenants.routes.ts     ← lazy routes: /new, /select
    ├── data-access/
    │   └── tenant.service.ts ← getMyTenants, createTenant, switchTenant
    ├── guards/
    │   └── tenant.guard.ts   ← multi-tenant routing logic
    ├── tenant-new/           ← TenantNewComponent
    └── tenant-select/        ← TenantSelectComponent
```

## Roteamento

```
/admin (AdminShellComponent)    ← authGuard + globalAdminGuard
  └── ''  → AdminTenantsComponent
/ (ShellComponent)              ← authGuard + tenantGuard
  ├── ''  → redirect /payments
  ├── doctors/                  ← DoctorsListComponent (lazy)
  ├── health-plans/             ← HealthPlansListComponent (lazy)
  ├── procedures/               ← ProceduresListComponent (lazy)
  ├── payments/                 ← PaymentsListComponent (lazy)
  └── members/                  ← MembersComponent (lazy)
/auth/**                        ← sem guards
/tenants/**                     ← authGuard
/**                             → redirect /auth/login
```

`tenantGuard`: 0 tenants + globalRole=Admin → redirect `/admin`; 0 tenants regular → redirect `/tenants/new`

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

## Armadilhas Conhecidas

### tenantGuard redireciona infinitamente após seleção de organização

- **Problema**: `tenantGuard` sempre redirecionava para `/tenants/select` quando o usuário tinha 2+ tenants, mesmo após ter selecionado um. Além disso, `CurrentUserService` tem cache em memória — após `switchTenant`, o cache ainda tinha o `UserDto` antigo (sem `tenantRole`), então o guard não conseguia detectar que o tenant já havia sido selecionado.
- **Correto**: (1) o `tenantGuard` verifica `user.tenantRole` antes de redirecionar: se não é nulo, o JWT já tem um tenant selecionado → `return of(true)`; (2) `TenantSelectComponent` chama `currentUserService.invalidate()` após `switchTenant` bem-sucedido, garantindo que a próxima chamada ao guard busque dados frescos do backend com o `tenantRole` correto.

### Sidebar mostra botões de admin logo após o login (role/tenant não aparecem)

- **Problema**: o JWT emitido no login (magic link / Google) não inclui `tenantRole`. O `tenantGuard` chama `switchTenant` para tenants únicos — mas o `CurrentUserService` mantém o cache do `getMe()` feito *antes* do switch, que tem `tenantRole: null`. O shell renderiza com dados antigos: `isDoctor()` retorna false, todos os botões aparecem, e o user card fica sem tenant/role.
- **Correto**: no `tenantGuard`, após `switchTenant`, chamar `currentUser.invalidate()` + `currentUser.getMe()` antes de retornar `true`. Isso garante que o cache é populado com os dados frescos (JWT atualizado com `tenantRole`) antes do shell ser ativado.

### Cloudflare Pages _redirects não suporta POST (usar Pages Function)
- **Problema**: a regra de proxy `status 200` no `_redirects` só encaminha GET. Qualquer `POST` (como `/api/auth/google/callback`) retorna 405, o error handler do Angular redireciona para `/auth/login` — sintoma: tela pisca e volta para o login.
- **Correto**: usar Cloudflare Pages Function em `functions/api/[[path]].js` (raiz do repo). A Function suporta todos os métodos HTTP e encaminha body, headers e cookies corretamente. O `_redirects` não deve ter regra `/api/*`.

### Magic Link Trampoline — token é de uso único

- **Comportamento**: `MagicLinkCallbackComponent` tenta abrir `medcontrol://verify?token=xxx` via `window.location.href` e escuta `visibilitychange`. Se o app abrir, o browser fica oculto (`hidden`) e o componente para sem chamar o backend.
- **Fallback**: após 2500ms sem `visibilitychange`, chama `verifyMagicLink(token)` normalmente (fluxo web).
- **Armadilha**: nunca chamar `verifyMagicLink` e tentar o deep link ao mesmo tempo — o token Redis é destruído na primeira chamada a `/auth/magic-link/verify`.
- **Testes**: usar `Proxy` sobre o `document` real para interceptar `visibilityState`/`addEventListener`/`removeEventListener` sem quebrar os internals do Angular (que também usam `DOCUMENT`).

### Token expirado (401) deve redirecionar para /auth/login, não para /tenants/new
- **Problema**: o `tenantGuard` chamava `GET /api/tenants/me`; se recebia 401 (token expirado), o `catchError` genérico redirecionava para `/tenants/new` ("criar organização") em vez de para o login. Além disso, não havia tratamento centralizado para 401 em requisições feitas enquanto o usuário já estava na página.
- **Correto**: (1) o `tenantGuard` distingue `HttpErrorResponse` com `status === 401` → `/auth/login` de outros erros → `/tenants/new`; (2) o `authInterceptor` captura `router = inject(Router)` no corpo da função (não dentro do `catchError`) e navega para `/auth/login` em qualquer 401 de endpoints `/api/` que não sejam `/api/auth/**`.

### fileReplacements obrigatório na configuração production do project.json
- **Problema**: sem `fileReplacements`, o build com `--configuration=production` ainda usa `environment.ts` (dev). Variáveis como `googleRedirectUri` ficam `null` e comportamentos dependem de fallbacks incorretos (ex: `window.location.origin` retorna URL de preview do Cloudflare).
- **Correto**: a configuração `production` em `project.json` deve sempre incluir:
  ```json
  "fileReplacements": [
    { "replace": "apps/web/src/environments/environment.ts",
      "with": "apps/web/src/environments/environment.production.ts" }
  ]
  ```
