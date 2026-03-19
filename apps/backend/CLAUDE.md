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
│   ├── Tenants/          ← Tenant, TenantMember, TenantRole (enum), ITenantRepository, Events/
│   ├── Users/            ← User, GlobalRole (enum), IUserRepository, Events/
│   ├── Doctors/          ← DoctorProfile, IDoctorRepository
│   ├── HealthPlans/      ← HealthPlan, IHealthPlanRepository
│   └── Procedures/       ← Procedure, IProcedureRepository
├── MedControl.Application/
│   ├── Mediator/         ← IMediator, Mediator, ICommand, IQuery, IRequest, Unit, IPipelineBehavior, IDomainEventHandler
│   ├── Behaviors/        ← LoggingBehavior, ValidationBehavior, TransactionBehavior
│   ├── Auth/
│   │   ├── Commands/     ← SendMagicLinkCommand(Handler+Validator), VerifyMagicLinkCommand(Handler+Validator),
│   │   │                    GoogleLoginCommand(Handler+Validator)
│   │   ├── DTOs/         ← AuthTokenDto
│   │   └── Settings/     ← MagicLinkSettings
│   ├── Doctors/
│   │   ├── Commands/     ← CreateDoctorCommand(Handler+Validator), UpdateDoctorCommand(Handler+Validator)
│   │   ├── Queries/      ← GetDoctorsQuery(Handler)
│   │   └── DTOs/         ← DoctorDto
│   ├── HealthPlans/
│   │   ├── Commands/     ← CreateHealthPlanCommand(Handler+Validator), UpdateHealthPlanCommand(Handler+Validator)
│   │   ├── Queries/      ← GetHealthPlansQuery(Handler)
│   │   └── DTOs/         ← HealthPlanDto
│   ├── Procedures/
│   │   ├── Commands/     ← CreateProcedureCommand(Handler+Validator), UpdateProcedureCommand(Handler+Validator),
│   │   │                    ImportProceduresCommand(Handler+Validator)
│   │   ├── Queries/      ← GetProceduresQuery(Handler), GetProcedureImportsQuery(Handler)
│   │   ├── Parsers/      ← IProcedureFileParser, ParsedProcedureRow, ParseResult
│   │   └── DTOs/         ← ProcedureDto, ProcedureImportDto
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
│       ├── Repositories/ ← UserRepository, TenantRepository, DoctorRepository, HealthPlanRepository,
│       │                    ProcedureRepository, ProcedureImportRepository
│       └── Configurations/ ← UserConfiguration, TenantConfiguration, TenantMemberConfiguration,
│                              DoctorProfileConfiguration, HealthPlanConfiguration,
│                              ProcedureConfiguration, ProcedureImportConfiguration
└── MedControl.Api/
    ├── Program.cs              ← ~15 linhas, sem AddControllers
    ├── Endpoints/
    │   ├── Auth/               ← MagicLinkEndpoints, GoogleAuthEndpoints (minimal API)
    │   ├── Doctors/            ← DoctorEndpoints (minimal API)
    │   └── EndpointExtensions  ← MapApiEndpoints(WebApplication)
    └── Extensions/
        ├── ServiceCollectionExtensions  ← AddApiServices (JWT bearer + ProblemDetails)
        └── ExceptionHandlerExtensions   ← UseApiExceptionHandler (ValidationException→400)

tests/
├── MedControl.Domain.Tests/         ← ResultTests, ErrorTests, TenantTests, UserTests ✅
├── MedControl.Architecture.Tests/   ← ArchitectureTests (NetArchTest) ✅
├── MedControl.Application.Tests/    ← validator + handler tests (Auth/, Doctors/) ✅
├── MedControl.Infrastructure.Tests/ ← model metadata + Auth service tests ✅
└── MedControl.Api.Tests/            ← MagicLinkEndpointTests, GoogleAuthEndpointTests,
                                        DoctorEndpointsTests (WebApplicationFactory) ✅
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

    public static class Errors { NameRequired, MemberAlreadyExists, MemberNotFound, InvalidRole }

    public static Result<Tenant> Create(string name)         // Raises TenantCreatedEvent
    public Result Update(string name)
    public Result AddMember(Guid userId, TenantRole role)    // validates Enum.IsDefined
    public Result RemoveMember(Guid userId)
    public void Deactivate()
}

public enum TenantRole { Admin = 0, Operator = 1, Doctor = 2 }
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

### HealthPlan

```csharp
public sealed class HealthPlan : BaseAuditableEntity, IAggregateRoot, IHasTenant
{
    private HealthPlan() { }  // EF Core
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }      // max 256
    public string TissCode { get; private set; }  // max 20, código ANS

    public static class Errors { NameRequired, TissCodeRequired }

    public static Result<HealthPlan> Create(Guid tenantId, string name, string tissCode)
    public Result Update(string name, string tissCode)
}

// IHealthPlanRepository
Task<IReadOnlyList<HealthPlan>> ListAsync(CancellationToken ct = default);
Task<HealthPlan?> GetByIdAsync(Guid id, CancellationToken ct = default);
Task<bool> ExistsByTissCodeAsync(Guid tenantId, string tissCode, CancellationToken ct = default);
Task AddAsync(HealthPlan healthPlan, CancellationToken ct = default);
Task UpdateAsync(HealthPlan healthPlan, CancellationToken ct = default);
```

### Procedure

```csharp
public sealed class Procedure : BaseAuditableEntity, IAggregateRoot, IHasTenant
{
    private Procedure() { }  // EF Core
    public Guid TenantId { get; private set; }
    public string Code { get; private set; }        // max 50, TUSS/CBHPM
    public string Description { get; private set; } // max 512
    public decimal Value { get; private set; }      // > 0, numeric(18,2)
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public ProcedureSource Source { get; private set; }  // Manual | Tuss | Cbhpm

    public static class Errors { CodeRequired, DescriptionRequired, ValueInvalid, EffectiveDateRangeInvalid }

    public static Result<Procedure> Create(Guid tenantId, string code, string description, decimal value,
        DateOnly effectiveFrom, DateOnly? effectiveTo = null, ProcedureSource source = Manual)
    public Result Update(string code, string description, decimal value, DateOnly? effectiveTo = null)
    public void CloseVigencia(DateOnly closedOn)
}

// IProcedureRepository
Task<IReadOnlyList<Procedure>> ListAsync(bool activeOnly, CancellationToken ct = default);
Task<Procedure?> GetByIdAsync(Guid id, CancellationToken ct = default);
Task<bool> ExistsByCodeAsync(Guid tenantId, string code, CancellationToken ct = default);
Task<bool> ExistsByCodeAndEffectiveFromAsync(Guid tenantId, string code, DateOnly effectiveFrom, CancellationToken ct = default);
Task AddAsync(Procedure procedure, CancellationToken ct = default);
Task AddRangeAsync(IEnumerable<Procedure> procedures, CancellationToken ct = default);
Task UpdateAsync(Procedure procedure, CancellationToken ct = default);
```

### ProcedureImport

```csharp
public sealed class ProcedureImport : BaseAuditableEntity, IAggregateRoot, IHasTenant
{
    public Guid TenantId { get; private set; }
    public ProcedureSource Source { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public int TotalRows { get; private set; }
    public int ImportedRows { get; private set; }
    public int SkippedRows { get; private set; }
    public string? ErrorSummary { get; private set; }  // max 2000

    public static class Errors { ManualSourceNotAllowed }

    public static Result<ProcedureImport> Create(Guid tenantId, ProcedureSource source, DateOnly effectiveFrom,
        int totalRows, int importedRows, int skippedRows, string? errorSummary)
}

// IProcedureImportRepository
Task AddAsync(ProcedureImport import, CancellationToken ct = default);
Task<IReadOnlyList<ProcedureImport>> ListAsync(CancellationToken ct = default);
```

### DoctorProfile

```csharp
public sealed class DoctorProfile : BaseAuditableEntity, IHasTenant
{
    private DoctorProfile() { }  // EF Core
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public string Crm { get; private set; }
    public string CouncilState { get; private set; }  // 2-char UF (e.g. "SP")
    public string Specialty { get; private set; }
    public string Name { get; private set; }

    public static class Errors { CrmRequired, SpecialtyRequired, CouncilStateRequired, NameRequired, CrmAlreadyExists }

    public static Result<DoctorProfile> Create(Guid tenantId, string name,
                                                string crm, string councilState, string specialty)
    public Result Update(string name, string crm, string councilState, string specialty)
}

// IDoctorRepository
Task<bool> ExistsByCrmAndTenantAsync(string crm, Guid tenantId, CancellationToken ct = default);
Task AddAsync(DoctorProfile doctor, CancellationToken ct = default);
Task<IReadOnlyList<DoctorProfile>> ListAsync(CancellationToken ct = default);
```

### Payment

```csharp
public sealed class Payment : BaseAuditableEntity, IAggregateRoot, IHasTenant
{
    private Payment() { }  // EF Core
    public Guid TenantId { get; private set; }
    public Guid DoctorId { get; private set; }        // FK → doctor_profiles
    public Guid HealthPlanId { get; private set; }    // FK → health_plans
    public DateOnly ExecutionDate { get; private set; }
    public string AppointmentNumber { get; private set; }  // max 100
    public string? AuthorizationCode { get; private set; } // max 100
    public string BeneficiaryCard { get; private set; }    // max 50
    public string BeneficiaryName { get; private set; }    // max 256
    public string ExecutionLocation { get; private set; }  // max 256
    public string PaymentLocation { get; private set; }    // max 256
    public string? Notes { get; private set; }
    public IReadOnlyList<PaymentItem> Items { get; }       // mínimo 1

    public static class Errors { AppointmentNumberRequired, BeneficiaryCardRequired, BeneficiaryNameRequired,
                                  ExecutionLocationRequired, PaymentLocationRequired, ItemsRequired, ItemNotFound,
                                  MinimumItemsRequired }

    public static Result<Payment> Create(Guid tenantId, Guid doctorId, Guid healthPlanId, DateOnly executionDate,
        string appointmentNumber, string? authorizationCode, string beneficiaryCard, string beneficiaryName,
        string executionLocation, string paymentLocation, string? notes,
        IEnumerable<(Guid ProcedureId, decimal Value)> items)
    public Result Update(DateOnly executionDate, string appointmentNumber, string? authorizationCode,
        string beneficiaryCard, string beneficiaryName, string executionLocation, string paymentLocation, string? notes)
    public Result<PaymentItem> GetItem(Guid itemId)
    public Result AddItem(Guid procedureId, decimal value)
    public Result RemoveItem(Guid itemId)
}

public sealed class PaymentItem : BaseEntity
{
    private PaymentItem() { }  // EF Core
    public Guid PaymentId { get; private set; }
    public Guid ProcedureId { get; private set; }    // FK → procedures
    public decimal Value { get; private set; }       // snapshot; numeric(18,2)
    public PaymentStatus Status { get; private set; } // Pending | Paid | Refused
    public string? Notes { get; private set; }

    public static class Errors { ValueInvalid }

    public Result UpdateStatus(PaymentStatus status, string? notes = null)
}

public enum PaymentStatus { Pending = 0, Paid = 1, Refused = 2, PartiallyPending = 3, PartiallyRefused = 4 }

// IPaymentRepository
Task<IReadOnlyList<Payment>> ListAsync(CancellationToken ct = default);
Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default);
Task AddAsync(Payment payment, CancellationToken ct = default);
Task UpdateAsync(Payment payment, CancellationToken ct = default);
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

- DbSets: `Tenants`, `TenantMembers`, `Users`, `DoctorProfiles`, `HealthPlans`, `Procedures`
- Interceptors: `AuditableEntityInterceptor`, `DomainEventDispatchInterceptor`
- Global query filters: `TenantMembers`, `DoctorProfiles`, `HealthPlans` e `Procedures` filtrados por `currentUser.TenantId`
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
UserRepository      : GetByIdAsync, GetByEmailAsync, AddAsync, UpdateAsync
TenantRepository    : GetByIdAsync (Include Members), GetBySlugAsync, AddAsync, UpdateAsync
                      ListByUserAsync → usa .IgnoreQueryFilters() para bypass do filtro multi-tenant
DoctorRepository    : ExistsByCrmAsync(tenantId, crm, councilState), AddAsync, ListAsync, GetByIdAsync, UpdateAsync
                      → global query filter cuida do escopo de tenant automaticamente no ListAsync
HealthPlanRepository: ExistsByTissCodeAsync(tenantId, tissCode), AddAsync, ListAsync, GetByIdAsync, UpdateAsync
                      → global query filter cuida do escopo de tenant automaticamente no ListAsync
ProcedureRepository : ExistsByCodeAsync(tenantId, code), ExistsByCodeAndEffectiveFromAsync(tenantId, code, effectiveFrom),
                      AddAsync, AddRangeAsync, ListAsync(activeOnly), GetByIdAsync, UpdateAsync
                      → global query filter cuida do escopo de tenant automaticamente no ListAsync
                      → ListAsync(activeOnly=true) filtra EffectiveFrom <= today AND (EffectiveTo IS NULL OR EffectiveTo >= today)
ProcedureImportRepository : AddAsync, ListAsync (ordered by CreatedAt desc)
                      → global query filter cuida do escopo de tenant automaticamente

Parsers (`Infrastructure/Procedures/Parsers/`):
TussCsvParser   : implementa IProcedureFileParser para ProcedureSource.Tuss
                  colunas: CD_TUSS;DS_TERMO;VL_PORTE;DT_VIG_INICIO;DT_VIG_FIM
                  EffectiveTo = coluna DT_VIG_FIM em dd/MM/yyyy (vazio = null)
CbhpmCsvParser  : implementa IProcedureFileParser para ProcedureSource.Cbhpm
                  colunas: CÓDIGO;NOMENCLATURA;PORTE;CUSTO_OPERACIONAL
                  Value = PORTE + CUSTO_OPERACIONAL; EffectiveTo = null sempre
```

### Entity Configurations (`Persistence/Configurations/`)

snake_case em tudo (convenção PostgreSQL). Tabelas: `users`, `tenants`, `tenant_members`.

| Entidade | Destaques |
|---|---|
| `UserConfiguration` | `avatar_url`: `Uri → string` converter, max 2048; índice único `ix_users_email` |
| `TenantConfiguration` | índice único `ix_tenants_slug`; `Members` com `PropertyAccessMode.Field` |
| `TenantMemberConfiguration` | FK→Tenant: `Cascade`; FK→User: `Restrict`; índice composto único `(tenant_id, user_id)` + índice simples `user_id`; `Role`: `HasConversion<string>()`, max 50 |
| `DoctorProfileConfiguration` | Tabela `doctor_profiles`; índice único `(crm, council_state, tenant_id)`; global query filter por `tenant_id`; `CA1861` suprimido no arquivo de migration |
| `HealthPlanConfiguration` | Tabela `health_plans`; índice único `ix_health_plans_tenant_tiss_code` em `(tenant_id, tiss_code)`; índice simples `ix_health_plans_tenant_id`; `CA1861` suprimido no arquivo de migration |
| `ProcedureConfiguration` | Tabela `procedures`; índice único `ix_procedures_tenant_code_effective_from` em `(tenant_id, code, effective_from)`; índice `ix_procedures_tenant_effective_dates` em `(tenant_id, effective_from, effective_to)`; índice simples `ix_procedures_tenant_id`; `value`: `numeric(18,2)`; `source`: `varchar(20)` |
| `ProcedureImportConfiguration` | Tabela `procedure_imports`; índice simples `ix_procedure_imports_tenant_id`; global query filter por `tenant_id` |

Todas as PKs: `ValueGeneratedNever()` — IDs gerados pela aplicação.

### Migrations

- `ApplicationDbContextFactory` (design-time only) em `Persistence/` — permite rodar `dotnet ef` sem DI completo
- Migration atual: `InitialSchema` — cria `tenants`, `users`, `tenant_members` com todos os índices

> ⚠️ **Após cada `dotnet ef migrations add`, execute imediatamente `dotnet format`.**
> O gerador do EF Core produz arquivos com block-scoped namespace, BOM e CRLF — todos rejeitados pelo `--warnaserror` do projeto. O `dotnet format` corrige os três de uma vez. Nunca edite os arquivos de migration manualmente para corrigir esses erros.

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

var doctors = app.MapGroup("doctors");
doctors.MapDoctors();
```

### Endpoints Disponíveis

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| `POST` | `/auth/magic-link/send` | ❌ | Envia magic link; cria usuário se não existir → 204 |
| `POST` | `/auth/magic-link/verify` | ❌ | Valida token; retorna JWT + refresh token → 200 |
| `POST` | `/auth/google/callback` | ❌ | Troca code Google por JWT; cria usuário se não existir → 200 |
| `GET` | `/doctors` | ✅ | Lista médicos do tenant; retorna `DoctorDto[]` → 200 |
| `POST` | `/doctors` | ✅ | Cria médico; verifica CRM duplicado → 201 / 409 |
| `PATCH` | `/doctors/{id}` | ✅ | Atualiza médico; verifica CRM duplicado → 200 / 404 / 409 |
| `GET` | `/health-plans` | ✅ | Lista convênios do tenant; retorna `HealthPlanDto[]` → 200 |
| `POST` | `/health-plans` | ✅ | Cria convênio; verifica TissCode duplicado → 201 / 409 |
| `PATCH` | `/health-plans/{id}` | ✅ | Atualiza convênio; verifica TissCode duplicado → 200 / 404 / 409 |
| `GET` | `/procedures?activeOnly=true` | ✅ | Lista procedimentos do tenant; retorna `ProcedureDto[]` → 200 |
| `POST` | `/procedures` | ✅ | Cria procedimento com vigência; verifica code+effectiveFrom duplicado → 201 / 409 |
| `PATCH` | `/procedures/{id}` | ✅ | Atualiza procedimento; verifica code duplicado → 200 / 404 / 409 |
| `POST` | `/procedures/import` | ✅ | Importa CSV TUSS ou CBHPM; multipart/form-data: file, source, effectiveFrom → 200 / 400 |
| `GET` | `/procedures/imports` | ✅ | Lista histórico de importações do tenant → 200 |
| `GET` | `/users/me` | ✅ | Retorna dados do usuário logado → 200 / 401 / 404 |
| `PATCH` | `/users/me/profile` | ✅ | Atualiza displayName do usuário logado → 200 / 400 / 401 / 404 |
| `GET` | `/payments` | ✅ | Lista pagamentos do tenant; retorna `PaymentDto[]` → 200 |
| `POST` | `/payments` | ✅ | Cria pagamento com itens; mínimo 1 item → 201 / 400 |
| `GET` | `/payments/{id}` | ✅ | Busca pagamento por id com itens → 200 / 404 |
| `PATCH` | `/payments/{id}` | ✅ | Atualiza campos de cabeçalho do pagamento → 200 / 404 |
| `PATCH` | `/payments/{id}/items/{itemId}` | ✅ | Atualiza status do item (Pending/Paid/Refused) → 200 / 404 |
| `POST` | `/payments/{id}/items` | ✅ | Adiciona item ao pagamento → 201 / 400 / 404 |
| `DELETE` | `/payments/{id}/items/{itemId}` | ✅ | Remove item do pagamento; mínimo 1 item deve restar → 200 / 400 / 404 |

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

`TestWebApplicationFactory` substitui serviços de infraestrutura por mocks NSubstitute e injeta configuração via `ConfigureAppConfiguration` (não `UseSetting`, que vai para host config e não app config).

### Autenticação nos Testes de Integração

`TestAuthHandler` interpreta headers `X-Test-*` para montar a identidade:

| Header | Claim injetado |
|---|---|
| `X-Test-UserId` | `sub` |
| `X-Test-Email` | `email` |
| `X-Test-TenantId` | `tenant_id` |

`CreateAuthenticatedClient(userId, email, tenantId?)` cria um `HttpClient` com esses headers pré-configurados. O `tenantId` é necessário para qualquer endpoint que dependa de `ICurrentTenantService`.

### Mocks disponíveis em TestWebApplicationFactory

```csharp
public IUserRepository UserRepository { get; }      // NSubstitute mock
public ITenantRepository TenantRepository { get; }  // NSubstitute mock
public IDoctorRepository DoctorRepository { get; }  // NSubstitute mock
```

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

### Infraestrutura / Auth
- Troca de tenant (`POST /auth/switch-tenant`)
- Endpoints de tenant e usuário (CRUD)

### Controle de Pagamento (próximo milestone)

**Ordem de implementação recomendada:**

1. ~~**Tenant Roles**~~ — ✅ implementado (`TenantRole` enum: Admin/Operator/Doctor; validação no `AddMember` e `UpdateRole`)
2. ~~**DoctorProfile**~~ — ✅ implementado (`DoctorProfile`: CRM, CouncilState, Specialty; `IDoctorRepository`; `CreateDoctorCommand`; `UpdateDoctorCommand`; `GetDoctorsQuery`; endpoints `GET/POST/PATCH /doctors`)
3. ~~**HealthPlan**~~ — ✅ implementado (`HealthPlan`: name, tissCode; `IHealthPlanRepository`; `CreateHealthPlanCommand`; `UpdateHealthPlanCommand`; `GetHealthPlansQuery`; endpoints `GET/POST/PATCH /health-plans`)
4. ~~**Procedure**~~ — ✅ implementado (`Procedure`: code, description, value, vigências (effectiveFrom/effectiveTo), source; `ProcedureImport`; `IProcedureRepository`; `IProcedureImportRepository`; `IProcedureFileParser` (TUSS + CBHPM); `CreateProcedureCommand`; `UpdateProcedureCommand`; `ImportProceduresCommand`; `GetProceduresQuery(activeOnly)`; `GetProcedureImportsQuery`; endpoints `GET/POST/PATCH /procedures`, `POST /procedures/import`, `GET /procedures/imports`)
5. ~~**Payment**~~ — ✅ implementado (`Payment`: aggregate root com `PaymentItem`; `PaymentStatus` (Pending/Paid/Refused/PartiallyPending/PartiallyRefused, computed); `IPaymentRepository`; `CreatePaymentCommand`; `UpdatePaymentCommand`; `UpdatePaymentItemStatusCommand`; `AddPaymentItemCommand`; `RemovePaymentItemCommand`; `ListPaymentsQuery`; `GetPaymentQuery`; endpoints `GET/POST /payments`, `GET /payments/{id}`, `PATCH /payments/{id}`, `PATCH /payments/{id}/items/{itemId}`, `POST /payments/{id}/items`, `DELETE /payments/{id}/items/{itemId}`; migration `AddPaymentTables`)
6. **Report Query** — agrega pagamentos por período/convênio/status para o médico
7. **Paginação e filtros** — por doutor, convênio, status, período

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

---

## Auth — HttpOnly Cookies (atualizado)

Os endpoints de auth agora setam cookies HttpOnly em vez de retornar tokens no body.

### Cookies

| Cookie | HttpOnly | Descrição |
|---|---|---|
| `mmc_access_token` | ✅ | JWT access token — nunca acessível via JS |
| `mmc_refresh_token` | ✅ | Refresh token — nunca acessível via JS |
| `mmc_session` | ❌ | Indicador de sessão (`1`); lido pelo JS para verificar se está logado |

### Endpoints

- `POST /auth/magic-link/verify` → retorna 204 + Set-Cookie (não retorna body)
- `POST /auth/google/callback` → retorna 204 + Set-Cookie (não retorna body)
- `POST /auth/logout` → expira os 3 cookies com `Max-Age=0` (não requer autenticação)

### CORS

Configurado via `AddCors("WebApp")` em `ServiceCollectionExtensions`:
- `Cors:WebOrigin` em `appsettings.Development.json` = `http://localhost:4200`
- `AllowCredentials()` habilitado

### JWT Bearer lê do cookie

```csharp
options.Events = new JwtBearerEvents
{
    OnMessageReceived = ctx =>
    {
        if (ctx.Request.Cookies.TryGetValue("mmc_access_token", out var token))
        {
            ctx.Token = token;
        }
        return Task.CompletedTask;
    }
};
```

### Utilitário

`CookieHelper.SetAuthCookies(HttpContext, AuthTokenDto)` — seta os 3 cookies.
`CookieHelper.ClearAuthCookies(HttpContext)` — expira os 3 cookies.
