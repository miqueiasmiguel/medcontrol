# MedControl — Guia para o Claude

> Detalhes de implementação do backend em `apps/backend/CLAUDE.md`

## Domínio de Negócio

MedControl é um SaaS para médicos, clínicas, hospitais e empresas de faturamento médico.

### Módulos

| Módulo | Status | Descrição |
|---|---|---|
| **Controle de Pagamento** | ✅ backend + web implementados | Operador registra pagamentos; médico consulta via mobile |
| Faturamento | 🔜 futuro | Geração de lotes TISS, envio a convênios |
| Recurso de Glosa | 🔜 futuro | Contestação de pagamentos recusados |

### Bounded Contexts

```
Payments     ← aggregate root Payment (core domain) — backend implementado
             ← PaymentItem (entity filha): ProcedureId, Value (snapshot), Status (Pending/Paid/Refused)
             ← Payment.Status: computed (Pending/Paid/Refused/PartiallyPending/PartiallyRefused)
             ← endpoints: GET/POST /payments, GET /payments/{id}, PATCH /payments/{id}
             ←            PATCH /payments/{id}/items/{itemId}, POST /payments/{id}/items
             ←            DELETE /payments/{id}/items/{itemId}
             ← mobile: lista (app/(app)/index.tsx) + detalhe (app/(app)/payments/[id].tsx)
             ← PaymentService.getPayment(id) → GET /payments/{id}
             ← usePayment(id): { payment, loading, error, refetch }
             ← PaymentCard.onPress → router.push(`/payments/${id}`)
             ← convênio resolvido client-side via HealthPlanService.listHealthPlans()
Users        ← User.UpdateProfile() — backend + web implementados
             ← endpoints: GET /users/me, PATCH /users/me/profile
             ← web: /settings (ThemeService: light/dark/system via data-theme attr; SettingsService: getMe/updateProfile)
Members      ← gerenciamento de membros do tenant — backend + web implementados
             ← Tenant.UpdateMemberRole(userId, currentUserId, role) + Tenant.Errors.CannotUpdateOwnRole
             ← IUserRepository.GetByIdsAsync — busca múltiplos usuários por IDs
             ← endpoints: GET/POST /members, PATCH /members/{userId}, DELETE /members/{userId}
             ← permissão: role "admin" ou "owner" para POST/PATCH/DELETE
             ← web: /members (MembersComponent + MemberFormComponent + MembersService)
             ← TestAuthHandler: X-Test-Roles header (comma-separated) → claims "roles"
             ← CreateAuthenticatedClient: parâmetro opcional roles string[]
Doctors      ← DoctorProfile vinculado a User (CRM, especialidade, conselho)
             ← DoctorProfile.Errors.OnlyLinkedDoctorCanUpdate (Forbidden) — apenas o user vinculado pode editar
             ← PATCH /doctors/{id} bloqueia edição quando outro user está vinculado (403 Forbidden)
             ← GET /users/me/doctor-profile → DoctorDto? (200 empty body se não vinculado)
             ← PATCH /users/me/doctor-profile → IReadOnlyList<DoctorDto> (atualiza todos os perfis cross-tenant)
             ← mobile: app/(app)/settings.tsx → SettingsScreen (perfil + tema + logout)
             ← SettingsScreen: edita perfil + seleção de tema (sistema/claro/escuro) + botão sair
             ← ThemePreferenceProvider (src/contexts/ThemeContext.tsx): persiste preferência em AsyncStorage (mmc_theme)
             ← useAppTheme(): retorna tema correto baseado na preferência; substitui useTheme() de @medcontrol/design-system/native
             ← useThemePreference(): { preference, setPreference } — usado em SettingsScreen
             ← UserService.getDoctorProfile() + updateMyDoctorProfile() + updateProfile()
             ← useDoctorProfile(): { doctorProfile, loading, error, refetch }
             ← submit paralelo: Promise.all([updateProfile, updateMyDoctorProfile])
HealthPlans  ← Convênio (nome, código TISS)
Procedures   ← Procedimento (código TUSS/CBHPM, descrição, valor, vigências) — UI pronta, backend implementado
             ← ProcedureImport (histórico de importações CSV TUSS/CBHPM por tenant)
             ← vigências: EffectiveFrom/EffectiveTo por código; importação idempotente
```

### Roles de Tenant

| Role | Quem | Permissões |
|---|---|---|
| `operator` | Operador/secretaria | CRUD completo de pagamentos, cadastros |
| `doctor` | Médico | Leitura dos próprios pagamentos, relatórios |
| `admin` | Administrador do tenant | Gerencia membros e configurações |
| `owner` | Proprietário | Mesmas permissões de admin (role máximo) |

> Verificação de permissão nos handlers: `Roles.Contains("admin", OrdinalIgnoreCase) || Roles.Contains("owner", OrdinalIgnoreCase)`

### Payment — Campos

| Campo | Tipo | Descrição |
|---|---|---|
| DoctorId | Guid | Ref. ao User com role `doctor` |
| HealthPlanId | Guid | Convênio |
| ProcedureId | Guid | Procedimento (TUSS/CBHPM) |
| ExecutionDate | DateOnly | Data de execução do procedimento |
| AppointmentNumber | string | Número do atendimento |
| AuthorizationCode | string? | Senha de autorização do convênio |
| BeneficiaryCard | string | Carteira do beneficiário |
| BeneficiaryName | string | Nome do beneficiário |
| ExecutionLocation | string | Local de execução |
| PaymentLocation | string | Local de pagamento |
| Status | enum | `Pending` / `Paid` / `Refused` |
| Notes | string? | Observações |

---

## Stack

| Camada | Tecnologia | Versão |
|---|---|---|
| Backend | .NET / C# | 10 |
| ORM | Entity Framework Core | 10.0.5 |
| Web | Angular | latest |
| Mobile | React Native (Expo) | latest |
| Banco de dados | PostgreSQL | 16 |
| Cache | Upstash Redis (IDistributedCache) | — |
| Email | Resend SDK | — |
| Storage | Cloudflare R2 (S3-compat) | — |

## Estrutura de Pastas

```
medcontrol/
├── apps/backend/src/
│   ├── MedControl.Domain/[BoundedContext]/
│   ├── MedControl.Application/[BoundedContext]/Commands|Queries/
│   ├── MedControl.Infrastructure/
│   └── MedControl.Api/Controllers/
├── apps/web/src/app/[feature]/
├── apps/mobile/src/screens/[feature]/
└── packages/contracts/src/[domain]/
```

## Arquitetura Backend — Clean Architecture + DDD

```
Domain (sem dependências)
  ↓ Application (depende de Domain)
    ↓ Infrastructure (implementa interfaces de Application)
      ↓ Api (depende de Application + Infrastructure)
```

- Mediator customizado (sem MediatR). Pipeline: `LoggingBehavior → ValidationBehavior → TransactionBehavior → Handler`
- Result Pattern em toda a camada de domínio (sem exceptions em domain logic)
- Multi-tenancy via global query filter no EF Core por `TenantId`

## Autenticação

- **Magic Link**: token one-time em `IDistributedCache`, TTL 15 min
- **Google OAuth (web)**: `POST /auth/google/callback` — troca authorization code via `webClientId + clientSecret`, cria/atualiza User
- **Google OAuth (mobile)**: `POST /auth/google/verify` — verifica `id_token` via `GET https://oauth2.googleapis.com/tokeninfo` sem `client_secret`, cria/atualiza User
- **JWT**: claims `sub`, `email`, `tenant_id`, `roles`, `global_roles`
- Troca de tenant: `POST /auth/switch-tenant` re-emite JWT

## Regras de TDD — INEGOCIÁVEIS

1. **Nunca escrever implementação antes do teste** — use `/tdd`
2. Ciclo: RED → GREEN (mínimo código) → REFACTOR → COMMIT
3. Commit a cada ciclo completo (conventional commit)
4. Cobertura mínima: **80% backend**, **75% web**, **70% mobile**
5. Architecture tests com NetArchTest devem sempre passar

## Convenções .NET

```csharp
// ✅ Factory method retorna Result<T>
public static Result<Tenant> Create(string name) { ... }

// ✅ Records para commands/queries
public record CreateTenantCommand(string Name) : ICommand<TenantDto>;

// ✅ Handlers sealed, private setters, private ctor para EF
// ❌ throw em domain logic | public setters | lógica em controllers | DbContext fora de Infrastructure
```

## Convenções TypeScript/Angular

```typescript
// ✅ Strict mode | async pipe | OnPush | Reactive forms
// ❌ any | subscribe sem unsubscribe (usar async pipe ou takeUntilDestroyed)
```

## Comandos Disponíveis

| Comando | Quando usar |
|---|---|
| `/tdd` | Antes de qualquer nova implementação |
| `/feature` | Para criar nova bounded context / feature completa |
| `/review` | Antes de abrir PR |
| `/debug` | Quando teste ou build falha |
| `/commit` | Para gerar mensagem conventional commit |
| `/migration` | Para criar/aplicar EF Core migration |
| `/arch` | Para validar decisão de arquitetura |
| `/pr` | Para criar pull request (checks, título, body, `gh pr create`) |

## Conventional Commits — Regras do commitlint

O hook de commit valida a mensagem com `commitlint`. Regras obrigatórias:

- **scope** deve ser um de: `domain`, `app`, `infra`, `api`, `web`, `mobile`, `contracts`, `ci`, `deps`, `auth`, `tenants`, `users`, `payments`, `doctors`, `health-plans`, `procedures`, `members`
- **subject** deve ser 100% minúsculo — sem exceções, incluindo siglas e nomes de arquivo

```bash
# ✅ correto
feat(infra): add ef core entity configurations
fix(users): handle null avatar url in converter
docs(infra): update backend docs with migration info

# ❌ errado — maiúsculas no subject
feat(infra): add EF Core entity configurations   # "EF Core" → "ef core"
docs(infra): update backend CLAUDE.md            # "CLAUDE.md" → "claude.md"
feat(infra): configure tenant_member FKs         # "FKs" → "fks"
```

## Comandos de Dev Frequentes

```bash
# Backend (de apps/backend/)
dotnet build --warnaserror
dotnet test
dotnet format --verify-no-changes
dotnet ef migrations add NomeMigration \
  --project src/MedControl.Infrastructure \
  --startup-project src/MedControl.Api

# Frontend / Monorepo (da raiz)
pnpm nx serve web
pnpm nx serve mobile
pnpm nx affected:test
pnpm nx affected:lint
```

## Fluxo de Feature

```
git checkout -b feat/nome
/tdd → red → green → refactor → commit (várias vezes)
/review → git push origin feat/nome → PR → CI verde → merge
```

## Antes de Cada Commit

- [ ] `dotnet test` / `pnpm nx test` passando
- [ ] `dotnet build --warnaserror` sem warnings
- [ ] `dotnet format --verify-no-changes` / `pnpm nx lint` limpos
- [ ] Cobertura >= threshold
- [ ] `/review` executado

## Variáveis de Ambiente

```bash
DATABASE_URL=postgresql://...
JWT_SECRET=...                    # min 256 bits
JWT_ISSUER=https://medcontrol.app
JWT_AUDIENCE=medcontrol-api
GOOGLE_CLIENT_ID=...
GOOGLE_CLIENT_SECRET=...
RESEND_API_KEY=re_...
UPSTASH_REDIS_URL=rediss://...
UPSTASH_REDIS_TOKEN=...
R2_ACCOUNT_ID=...
R2_ACCESS_KEY_ID=...
R2_SECRET_ACCESS_KEY=...
R2_BUCKET_NAME=medcontrol-uploads
```
