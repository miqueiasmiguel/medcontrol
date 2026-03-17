# MedControl Backend

## Stack

- .NET 10 / C# | EF Core 10.0.5 | PostgreSQL 16
- FluentValidation 11 | xUnit + FluentAssertions | NetArchTest.Rules
- Npgsql | StackExchange.Redis | JwtBearer 10.0.5 | Scalar

## Estrutura de Pastas

```
src/
├── MedControl.Domain/
│   ├── Common/           ← BaseEntity, BaseAuditableEntity, Result, Error, IAggregateRoot, IDomainEvent, IHasTenant
│   ├── Auth/             ← AuthProvider (enum)
│   ├── Tenants/          ← Tenant, TenantMember, ITenantRepository, Events/
│   └── Users/            ← User, GlobalRole (enum), IUserRepository, Events/
├── MedControl.Application/
│   ├── Mediator/         ← IMediator, Mediator, ICommand, IQuery, IRequest, Unit, IPipelineBehavior, IDomainEventHandler
│   ├── Behaviors/        ← LoggingBehavior, ValidationBehavior, TransactionBehavior
│   └── Common/
│       ├── Interfaces/   ← IUnitOfWork, ICurrentTenantService, ICurrentUserService,
│       │                    IEmailService, ITokenService, IMagicLinkService
│       └── Exceptions/   ← NotFoundException
├── MedControl.Infrastructure/
│   └── Persistence/
│       ├── ApplicationDbContext.cs
│       ├── Interceptors/ ← AuditableEntityInterceptor, DomainEventDispatchInterceptor
│       ├── Repositories/ ← UserRepository, TenantRepository
│       └── Configurations/ ← UserConfiguration, TenantConfiguration, TenantMemberConfiguration ✅
└── MedControl.Api/
    ├── Program.cs
    └── Controllers/      (vazio)

tests/
├── MedControl.Domain.Tests/         ← ResultTests, ErrorTests, TenantTests, UserTests ✅
├── MedControl.Architecture.Tests/   ← ArchitectureTests (NetArchTest) ✅
├── MedControl.Application.Tests/    (vazio — NSubstitute)
├── MedControl.Infrastructure.Tests/ ← model metadata tests (32 testes) ✅
└── MedControl.Api.Tests/            (vazio — WebApplicationFactory)
```

---

## Arquitetura

```
Domain (sem dependências)
  ↓ Application (depende de Domain)
    ↓ Infrastructure (implementa interfaces de Application)
      ↓ Api (depende de Application + Infrastructure)
```

**Enforced por NetArchTest** — architecture tests devem sempre passar.

---

## Domain Layer

### Result Pattern

```csharp
// Error: record imutável
public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new("", "");
    public static readonly Error NullValue = new("Error.NullValue", "...");
}

// Result<T>: acesso a Value lança se IsFailure
public static implicit operator Result<T>(T value) => Result.Success(value); // conversão implícita
```

### Hierarquia de Entidades

```
BaseEntity                          // Id (Guid.NewGuid()), DomainEvents, Raise(), ClearDomainEvents()
  └── BaseAuditableEntity           // + CreatedAt/By, UpdatedAt/By (preenchidos pelo interceptor)
        ├── Tenant : IAggregateRoot
        └── User   : IAggregateRoot
TenantMember : BaseEntity           // child entity — não é aggregate root
```

### Tenant

```csharp
public sealed class Tenant : BaseAuditableEntity, IAggregateRoot
{
    private Tenant() { }  // EF Core
    public string Name { get; private set; }
    public string Slug { get; private set; }   // lowercase, spaces→dashes
    public bool IsActive { get; private set; } = true
    public IReadOnlyList<TenantMember> Members { get; }

    public static class Errors { NameRequired, MemberAlreadyExists, MemberNotFound, RoleRequired }

    public static Result<Tenant> Create(string name)    // Raises TenantCreatedEvent
    public Result Update(string name)
    public Result AddMember(Guid userId, string role)
    public Result RemoveMember(Guid userId)
    public void Deactivate()
}
```

### User

```csharp
public sealed class User : BaseAuditableEntity, IAggregateRoot
{
    private User() { }  // EF Core
    public string Email { get; private set; }          // sempre lowercase
    public string? DisplayName { get; private set; }
    public Uri? AvatarUrl { get; private set; }
    public bool IsEmailVerified { get; private set; }  // false por padrão
    public GlobalRole GlobalRole { get; private set; } = GlobalRole.None
    public DateTimeOffset? LastLoginAt { get; private set; }

    public static class Errors { EmailRequired, DisplayNameRequired }

    public static Result<User> Create(string email, string? displayName = null)  // Raises UserRegisteredEvent
    public static Result<User> CreateFromGoogle(string email, string displayName, Uri? avatarUrl)  // IsEmailVerified = true
    public void UpdateProfile(string? displayName, Uri? avatarUrl)
    public void VerifyEmail() | RecordLogin() | SetGlobalRole(GlobalRole role)
    public bool IsGlobalAdmin() | IsGlobalSupport()
}

public enum GlobalRole { None = 0, Support = 1, Admin = 2 }
public enum AuthProvider { MagicLink = 0, Google = 1 }
```

### Domain Events

```csharp
record TenantCreatedEvent(Guid AggregateId, string TenantName, DateTimeOffset OccurredAt) : IDomainEvent;
record UserRegisteredEvent(Guid AggregateId, string Email, DateTimeOffset OccurredAt) : IDomainEvent;
```

---

## Application Layer

### Mediator Customizado (sem MediatR)

```csharp
IRequest<TResponse>                              // marker
ICommand : IRequest<Unit>                        // sem retorno
ICommand<TResponse> : IRequest<TResponse>        // com retorno
IQuery<TResponse> : IRequest<TResponse>          // read-only, sem transação
IRequestHandler<TRequest, TResponse>             // implementar nos handlers
IDomainEventHandler<TEvent>                      // para domain events
```

**Pipeline:** `LoggingBehavior → ValidationBehavior → TransactionBehavior → Handler`

- `TransactionBehavior` só envolve `ICommand` (não queries)
- Registro: `services.AddMediator(Assembly.GetAssembly(typeof(IMediator))!)`

### Interfaces de Application

```csharp
IUnitOfWork             // SaveChangesAsync, BeginTransactionAsync, CommitTransactionAsync, RollbackTransactionAsync
ICurrentUserService     // UserId?, TenantId?, Email, Roles, GlobalRoles, IsAuthenticated, HasGlobalRole(role)
ICurrentTenantService   // TenantId?, HasTenant
ITokenService           // GenerateTokenPair(...) → TokenPair(AccessToken, RefreshToken, ExpiresAt)
                        // ValidateRefreshTokenAsync, RevokeRefreshTokenAsync
IEmailService           // SendMagicLinkAsync(email, link, ct)
IMagicLinkService       // GenerateTokenAsync(email) → token | ValidateTokenAsync(token) → email?
```

---

## Infrastructure Layer

### ApplicationDbContext

- DbSets: `Tenants`, `TenantMembers`, `Users`
- Interceptors: `AuditableEntityInterceptor`, `DomainEventDispatchInterceptor`
- Global query filter: `TenantMembers` filtrado por `currentUser.TenantId`
- Implementa `IUnitOfWork` diretamente
- Connection string key: `"Database"` (Npgsql)

### Interceptors

```
AuditableEntityInterceptor      → preenche CreatedAt/By, UpdatedAt/By no SaveChanges
DomainEventDispatchInterceptor  → após SaveChanges: coleta eventos de BaseEntity,
                                  limpa lista, despacha IDomainEventHandler<T> via DI + reflection
```

### Repositories

```
UserRepository  : GetByIdAsync, GetByEmailAsync, AddAsync, UpdateAsync
TenantRepository: GetByIdAsync (Include Members), GetBySlugAsync, AddAsync, UpdateAsync
                  ListByUserAsync → usa .IgnoreQueryFilters() para bypass do filtro multi-tenant
```

### Entity Configurations (`Persistence/Configurations/`)

snake_case em tudo (convenção PostgreSQL). Tabelas: `users`, `tenants`, `tenant_members`.

| Entidade | Destaques |
|---|---|
| `UserConfiguration` | `avatar_url`: `Uri → string` converter, max 2048; índice único `ix_users_email` |
| `TenantConfiguration` | índice único `ix_tenants_slug`; `Members` com `PropertyAccessMode.Field` |
| `TenantMemberConfiguration` | FK→Tenant: `Cascade`; FK→User: `Restrict`; índice composto único `(tenant_id, user_id)` + índice simples `user_id` |

Todas as PKs: `ValueGeneratedNever()` — IDs gerados pela aplicação.

### Migrations

- `ApplicationDbContextFactory` (design-time only) em `Persistence/` — permite rodar `dotnet ef` sem DI completo
- Migration atual: `InitialSchema` — cria `tenants`, `users`, `tenant_members` com todos os índices

### Registro (InfrastructureExtensions.AddInfrastructure)

Registra: interceptors → DbContext (Npgsql) → IUnitOfWork → IUserRepository → ITenantRepository

---

## Multi-Tenancy

- `IHasTenant` marca entidades tenant-scoped
- Global query filter em `TenantMember` automático
- Bypass: `.IgnoreQueryFilters()` — somente para `GlobalRole`
- Nunca acessar dados de outro tenant sem `GlobalRole`

---

## Autenticação (a implementar em Infrastructure/Auth/)

- **Magic Link**: `IMagicLinkService` → `IDistributedCache`, TTL 15 min, one-time use
- **Google OAuth**: troca code → user info → `User.CreateFromGoogle()`
- **JWT**: claims `sub`, `email`, `tenant_id`, `roles`, `global_roles`
- Troca de tenant: `POST /auth/switch-tenant` re-emite JWT

---

## Convenções Obrigatórias

```csharp
// ✅ Factory method retorna Result<T>
public static Result<Tenant> Create(string name) { ... }

// ✅ Erros aninhados na entidade
public static class Errors { public static readonly Error NameRequired = new("Tenant.NameRequired", "..."); }

// ✅ Handlers são sealed
public sealed class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, TenantDto> { }

// ✅ Records para commands/queries
public record CreateTenantCommand(string Name) : ICommand<TenantDto>;

// ✅ Private setters + private ctor
private Tenant() { }
public string Name { get; private set; } = default!;

// ❌ throw em domain logic → usar Result.Failure
// ❌ public setters em entidades
// ❌ lógica de negócio em controllers
// ❌ DbContext fora de Infrastructure
```

---

## Architecture Tests — Regras Enforçadas

1. Domain: zero dependências externas
2. Application: nenhuma dep em Infrastructure/Api
3. Infrastructure: nenhuma dep em Api
4. Todos os `IRequestHandler` são `sealed`
5. Entities de domínio têm construtor privado sem parâmetros

---

## O que Ainda Não Foi Implementado

- Controllers (Api/Controllers/ vazio)
- Auth Infrastructure: JWT, MagicLink, Google OAuth (Infrastructure/Auth/ vazio)
- Application Handlers: nenhum command/query handler
- Testes de Application e Api (projetos vazios)

---

## Comandos

```bash
# Rodar de apps/backend/
dotnet build --warnaserror
dotnet test
dotnet test --filter "Category=Unit"
dotnet format --verify-no-changes
dotnet ef migrations add NomeMigration \
  --project src/MedControl.Infrastructure \
  --startup-project src/MedControl.Api
```
