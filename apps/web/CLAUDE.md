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
│   └── tokens/
│       └── window.token.ts  ← InjectionToken<Window> para mockabilidade em testes
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
│   ├── magic-link-callback/  ← MagicLinkCallbackComponent (lê ?token, chama verifyMagicLink → cookie)
│   └── google-callback/      ← GoogleCallbackComponent (troca code → cookie)
├── doctors/
│   ├── doctors.routes.ts     ← { path: '' → DoctorsListComponent }
│   ├── data-access/
│   │   └── doctor.service.ts ← getDoctors, createDoctor, updateDoctor; DoctorDto, CreateDoctorCommand
│   ├── doctors-list/         ← tabela de médicos + botão "Novo médico"; signals: doctors, formOpen, selectedDoctor
│   └── doctor-form/          ← slide-over panel; @Input doctor (null=criar); @Output saved/closed
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
/ (ShellComponent)              ← authGuard + tenantGuard
  ├── ''  → redirect /doctors
  ├── doctors/                  ← DoctorsListComponent (lazy)
  ├── health-plans/             ← HealthPlansListComponent (lazy)
  ├── procedures/               ← ProceduresListComponent (lazy)
  ├── payments/                 ← PaymentsListComponent (lazy)
  └── members/                  ← MembersComponent (lazy)
/auth/**                        ← sem guards
/tenants/**                     ← authGuard
/**                             → redirect /auth/login
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
