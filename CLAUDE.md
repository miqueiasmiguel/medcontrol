# MedControl — Guia para o Claude

## Stack & Versões

| Camada | Tecnologia | Versão |
|---|---|---|
| Backend | .NET / C# | 10 |
| ORM | Entity Framework Core | 9+ |
| Web | Angular | latest |
| Mobile | React Native (Expo) | latest |
| Banco de dados | PostgreSQL | 16 |
| Cache | Upstash Redis (IDistributedCache) | — |
| Email | Resend SDK | — |
| Storage | Cloudflare R2 (S3-compat) | — |

## Arquitetura

### Backend — Clean Architecture + DDD

```
Domain       → Application → Infrastructure
     ↑                ↑
     └────────────────┘
           API depende de Application
           Application depende de Domain
           Infrastructure depende de Application (implementa interfaces)
           NUNCA: Domain depende de qualquer outra camada
```

**Mediator customizado** (sem MediatR). Pipeline:
`LoggingBehavior → ValidationBehavior → TransactionBehavior → Handler`

**Multi-tenancy:** Global query filter no EF Core por `TenantId`.
- Entidades tenant-scoped implementam `IHasTenant`
- `TenantResolutionMiddleware` extrai tenant do JWT
- Roles globais usam `IgnoreQueryFilters()` para bypass

### Autenticação

- **Magic Link**: token one-time em `IDistributedCache`, TTL 15 min
- **Google OAuth**: troca de code, busca user info, cria/atualiza User
- **JWT** gerado internamente: claims `sub`, `email`, `tenant_id`, `roles`, `global_roles`
- Troca de tenant: `POST /auth/switch-tenant` re-emite JWT

## Regras de TDD — INEGOCIÁVEIS

1. **Nunca escrever implementação antes do teste** — use `/tdd`
2. Ciclo: RED (teste falha) → GREEN (mínimo código) → REFACTOR → COMMIT
3. Commit após cada ciclo (mensagem conventional commit)
4. Cobertura mínima: **80% backend**, **75% web**, **70% mobile**
5. Architecture tests com NetArchTest devem sempre passar

## Regras de Multi-Tenancy — SEMPRE VERIFICAR

- Toda entidade tenant-scoped implementa `IHasTenant`
- Queries multi-tenant usam global query filter (automático)
- Para bypass (global roles): chamar `.IgnoreQueryFilters()` explicitamente
- Nunca acessar dados de outro tenant sem `GlobalRole`

## Convenções .NET

```csharp
// ✅ Entities com factory method + domain events
public sealed class Tenant : BaseAuditableEntity, IAggregateRoot
{
    private Tenant() { } // EF Core
    public static Tenant Create(string name) { ... }
}

// ✅ Records para commands/queries
public record CreateTenantCommand(string Name) : ICommand<TenantDto>;

// ✅ Private setters em entidades
public string Name { get; private set; } = default!;

// ✅ Async/await em todos os handlers
public async Task<TenantDto> Handle(CreateTenantCommand cmd, CancellationToken ct)

// ❌ Nunca: public setters em entidades de domínio
// ❌ Nunca: lógica de negócio em controllers
// ❌ Nunca: EF Core DbContext fora de Infrastructure
```

## Convenções TypeScript/Angular

```typescript
// ✅ Strict mode sempre
// ✅ async pipe em templates (evita memory leaks)
// ✅ OnPush change detection em components
// ✅ Reactive forms para formulários complexos
// ❌ Nunca: any
// ❌ Nunca: subscribe sem unsubscribe (use async pipe ou takeUntilDestroyed)
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

## Comandos de Dev Frequentes

```bash
# Backend
cd apps/backend
dotnet build --warnaserror           # build (warnings = erros)
dotnet test                          # todos os testes
dotnet test --filter "Category=Unit" # só unitários
dotnet format --verify-no-changes    # lint
dotnet ef migrations add NomeMigration --project src/MedControl.Infrastructure \
  --startup-project src/MedControl.Api  # nova migration

# Frontend / Monorepo
pnpm nx serve web                    # Angular dev
pnpm nx serve mobile                 # Expo mobile
pnpm nx affected:test                # testa só o afetado
pnpm nx affected:lint                # lint só o afetado
```

## Fluxo de Feature (XP/TDD)

```
git checkout -b feat/nome            # branch curta (< 1 dia)
/tdd                                 # iniciar ciclo TDD
# ... red → green → refactor → commit (várias vezes)
/review                              # antes do PR
git push origin feat/nome
# Abrir PR → aguardar CI verde → merge
```

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

## Antes de Cada Commit

- [ ] Testes passando (`dotnet test` / `pnpm nx test`)
- [ ] Sem warnings de build (`--warnaserror`)
- [ ] Lint limpo (`dotnet format --verify-no-changes` / `pnpm nx lint`)
- [ ] Cobertura >= threshold
- [ ] `/review` executado

## Variáveis de Ambiente Necessárias

```bash
DATABASE_URL=postgresql://...
JWT_SECRET=...                       # min 256 bits
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
