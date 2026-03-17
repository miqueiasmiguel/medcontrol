# MedControl Backend

## Stack

- .NET 10 / C# | EF Core 10.0.5 | PostgreSQL 16
- FluentValidation 11 | xUnit + FluentAssertions | NetArchTest.Rules
- Npgsql | StackExchange.Redis | JwtBearer 10.0.5 | Resend 0.2.2 | Scalar

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
│   ├── Auth/
│   │   ├── Commands/     ← SendMagicLinkCommand(Handler+Validator), VerifyMagicLinkCommand(Handler+Validator),
│   │   │                    GoogleLoginCommand(Handler+Validator)
│   │   ├── DTOs/         ← AuthTokenDto
│   │   └── Settings/     ← MagicLinkSettings
│   └── Common/
│       ├── Interfaces/   ← IUnitOfWork, ICurrentTenantService, ICurrentUserService,
│       │                    IEmailService, ITokenService, IMagicLinkService, IGoogleAuthService
│       └── Exceptions/   ← NotFoundException
├── MedControl.Infrastructure/
│   ├── Auth/
│   │   ├── Settings/     ← JwtSettings, GoogleAuthSettings
│   │   ├── MagicLinkService.cs   ← IDistributedCache, token=RandomBytes(32) base64url, one-time
│   │   ├── TokenService.cs       ← HS256 JWT + refresh token em Redis
│   │   ├── GoogleAuthService.cs  ← HttpClient, troca code→token→userinfo via Google APIs
│   │   └── EmailService.cs       ← IResend (Resend 0.2.2)
│   ├── Http/
│   │   ├── HttpContextCurrentUserService.cs   ← lê claims JWT do HttpContext
│   │   └── HttpContextCurrentTenantService.cs ← lê tenant_id do HttpContext
│   └── Persistence/
│       ├── ApplicationDbContext.cs
│       ├── Interceptors/ ← AuditableEntityInterceptor, DomainEventDispatchInterceptor
│       ├── Repositories/ ← UserRepository, TenantRepository
│       └── Configurations/ ← UserConfiguration, TenantConfiguration, TenantMemberConfiguration
└── MedControl.Api/
    ├── Program.cs              ← ~15 linhas, sem AddControllers
    ├── Endpoints/
    │   ├── Auth/               ← MagicLinkEndpoints, GoogleAuthEndpoints (minimal API)
    │   └── EndpointExtensions  ← MapApiEndpoints(WebApplication)
    └── Extensions/
        ├── ServiceCollectionExtensions  ← AddApiServices (JWT bearer + ProblemDetails)
        └── ExceptionHandlerExtensions   ← UseApiExceptionHandler (ValidationException→400)

tests/
├── MedControl.Domain.Tests/         ← ResultTests, ErrorTests, TenantTests, UserTests ✅
├── MedControl.Architecture.Tests/   ← ArchitectureTests (NetArchTest) ✅
├── MedControl.Application.Tests/    ← validator + handler tests (Auth/) ✅
├── MedControl.Infrastructure.Tests/ ← model metadata + Auth service tests ✅
└── MedControl.Api.Tests/            ← MagicLinkEndpointTests, GoogleAuthEndpointTests (WebApplicationFactory) ✅
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

- `TransactionBehavior` envolve `ICommand` e `ICommand<TResponse>` (não queries)
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
IGoogleAuthService      // ExchangeCodeAsync(code, redirectUri) → GoogleUserInfo? { Email, DisplayName, AvatarUrl }
```

### MagicLinkSettings (Application/Auth/Settings/)

Definida em Application (não Infrastructure) para que os handlers possam referenciar via `IOptions<MagicLinkSettings>` sem violar a arquitetura.

```csharp
public sealed class MagicLinkSettings
{
    public const string SectionName = "MagicLink";
    public string BaseUrl { get; init; }        // URL base do frontend para o link
    public int TokenExpiryMinutes { get; init; } = 15;
}
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

### Auth Services (`Infrastructure/Auth/`)

| Serviço | Implementação |
|---|---|
| `MagicLinkService` | Gera token = `RandomBytes(32)` Base64Url; armazena `magic_link:{token}→email` no Redis com TTL 15 min; `ValidateTokenAsync` é one-time (remove após leitura) |
| `TokenService` | JWT HS256 com claims `sub`, `email`, `tenant_id`, `roles`, `global_roles`; refresh token em Redis com chave `refresh_token:{token}→userId` |
| `GoogleAuthService` | Registrado via `AddHttpClient<IGoogleAuthService, GoogleAuthService>()`; troca `code` pelo access token no endpoint `https://oauth2.googleapis.com/token`; busca userinfo em `https://www.googleapis.com/oauth2/v3/userinfo`; retorna `GoogleUserInfo` ou `null` em caso de falha |
| `EmailService` | Wraps `IResend` (Resend SDK 0.2.2) — registrado via `services.AddHttpClient<ResendClient>()` + `services.Configure<ResendClientOptions>(...)` + `services.AddTransient<IResend, ResendClient>()` |

### JwtSettings (`Infrastructure/Auth/Settings/`)

```csharp
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";
    public string Secret { get; init; }
    public string Issuer { get; init; }
    public string Audience { get; init; }
    public int AccessTokenExpiryMinutes { get; init; } = 60;
    public int RefreshTokenExpiryDays { get; init; } = 30;
}
```

### GoogleAuthSettings (`Infrastructure/Auth/Settings/`)

```csharp
public sealed class GoogleAuthSettings
{
    public const string SectionName = "Google";
    public string ClientId { get; init; }
    public string ClientSecret { get; init; }
}
```

### Http Services (`Infrastructure/Http/`)

`HttpContextCurrentUserService` e `HttpContextCurrentTenantService` — lêem claims JWT do `IHttpContextAccessor`. Corrigem o bug pré-existente onde `ApplicationDbContext` precisava de `ICurrentUserService` no DI mas ela não estava registrada.

### Registro (InfrastructureExtensions.AddInfrastructure)

Ordem: HttpContext → Settings → Redis → Auth services → Email (condicional por ambiente) → Persistence (interceptors → DbContext → IUnitOfWork → repos)

`IGoogleAuthService` registrado via `AddHttpClient<IGoogleAuthService, GoogleAuthService>()` (typed HttpClient).

---

## API Layer

### Minimal APIs (sem Controllers)

Endpoints definidos em `Endpoints/` como static classes com extension methods sobre `RouteGroupBuilder`.

```csharp
// EndpointExtensions.cs
var auth = app.MapGroup("auth");
auth.MapGroup("magic-link").MapMagicLink();
auth.MapGroup("google").MapGoogleAuth();
```

### Endpoints Disponíveis

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/auth/magic-link/send` | Envia magic link; cria usuário se não existir → 204 |
| `POST` | `/auth/magic-link/verify` | Valida token; retorna JWT + refresh token → 200 |
| `POST` | `/auth/google/callback` | Troca code Google por JWT; cria usuário se não existir → 200 |

### Mapeamento Result → IResult

```csharp
ErrorType.Unauthorized → 401
ErrorType.NotFound     → 404
ErrorType.Conflict     → 409
_                      → 400
```

### Exception Handling

`UseApiExceptionHandler()` em `ExceptionHandlerExtensions`:
- `ValidationException` → 400 com `Results.ValidationProblem(errors)`
- Qualquer outra exceção → 500

---

## Multi-Tenancy

- `IHasTenant` marca entidades tenant-scoped
- Global query filter em `TenantMember` automático
- Bypass: `.IgnoreQueryFilters()` — somente para `GlobalRole`
- Nunca acessar dados de outro tenant sem `GlobalRole`

---

## Autenticação

### Magic Link ✅
1. `POST /auth/magic-link/send` → normaliza email, cria usuário se não existe, gera token Redis, envia email via Resend
2. `POST /auth/magic-link/verify` → valida token (one-time), chama `VerifyEmail()` + `RecordLogin()`, retorna JWT pair

### Google OAuth ✅
1. `POST /auth/google/callback` `{ code, redirectUri }` → `GoogleLoginCommandHandler`
   - Chama `IGoogleAuthService.ExchangeCodeAsync(code, redirectUri)` → `GoogleUserInfo?`
   - Se null → `Error.Unauthorized("Auth.GoogleAuthFailed")` → 401
   - Se usuário não existe → `User.CreateFromGoogle(email, displayName, avatarUrl)` + `AddAsync`
   - Se usuário existe → `UpdateAsync`
   - Sempre chama `RecordLogin()` + `SaveChangesAsync`
   - Retorna JWT pair via `ITokenService.GenerateTokenPair`
2. `GoogleAuthService` (Infrastructure): typed `HttpClient`; POST para `https://oauth2.googleapis.com/token`; GET para `https://www.googleapis.com/oauth2/v3/userinfo`
3. Credenciais em `appsettings.json` seção `"Google": { "ClientId", "ClientSecret" }`

### JWT
- Claims: `sub`, `email`, `tenant_id`, `roles`, `global_roles`
- Troca de tenant: `POST /auth/switch-tenant` re-emite JWT (a implementar)

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
// ❌ lógica de negócio em controllers ou endpoints
// ❌ DbContext fora de Infrastructure
// ❌ AddControllers / MapControllers — usar Minimal APIs em Endpoints/
```

---

## Architecture Tests — Regras Enforçadas

1. Domain: zero dependências externas
2. Application: nenhuma dep em Infrastructure/Api
3. Infrastructure: nenhuma dep em Api
4. Todos os `IRequestHandler` são `sealed`
5. Entities de domínio têm construtor privado sem parâmetros

---

## Testes de Integração (Api.Tests)

`TestWebApplicationFactory` substitui serviços de infraestrutura por mocks NSubstitute e injeta configuração via `ConfigureAppConfiguration` (não `UseSetting`, que vai para host config e não app config):

```csharp
builder.ConfigureAppConfiguration((_, config) =>
{
    config.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["Jwt:Secret"] = "...",
        ["Jwt:Issuer"] = "...",
        ["Jwt:Audience"] = "...",
        ["ConnectionStrings:Database"] = "Host=localhost;Database=test",
        ["ConnectionStrings:Redis"] = "localhost",
    });
});
```

---

## O que Ainda Não Foi Implementado

- Troca de tenant (`POST /auth/switch-tenant`)
- Endpoints de tenant e usuário

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
