# /feature — Scaffold de Nova Feature

Use este comando para criar uma nova **bounded context** ou feature completa seguindo Clean Architecture + DDD.

## Git Workflow — Obrigatório

**Antes de criar qualquer arquivo**, crie uma branch:

```bash
git checkout -b feat/<nome-da-feature>
# Exemplos:
# feat/billing
# feat/appointments
# feat/notifications
```

**Durante a implementação**, faça commits granulares por camada (use `/tdd` internamente):

```bash
git commit -m "feat(<scope>): add <entity> domain aggregate"
git commit -m "feat(<scope>): add create <entity> command handler"
git commit -m "feat(<scope>): add <entity> repository and ef configuration"
git commit -m "feat(<scope>): add <entity> api endpoints"
```

**Ao final**, pergunte ao usuário:
> "Feature completa. Posso fazer o merge da branch `feat/<nome>` na `main` e apagá-la?"

---

Responda as perguntas abaixo antes de criar qualquer arquivo:

1. **Nome da feature/bounded context?** (ex: `Billing`, `Notifications`, `Appointments`)
2. **Aggregate root principal?** (ex: `Invoice`, `Notification`, `Appointment`)
3. **Entidades secundárias?** (ex: `InvoiceItem`, `InvoicePayment`)
4. **Precisa de tenant-scope?** (sim/não — quase sempre sim)

---

## Estrutura a criar (substitua `{Feature}` e `{Entity}`)

### 1. Domain Layer

```
apps/backend/src/MedControl.Domain/{Feature}/
├── {Entity}.cs                        ← Aggregate root
├── {EntitySecondary}.cs               ← entidades secundárias (se houver)
├── {Entity}Status.cs                  ← enums de domínio (se necessário)
├── I{Entity}Repository.cs             ← interface do repositório
└── Events/
    ├── {Entity}CreatedEvent.cs
    └── {Entity}UpdatedEvent.cs
```

**Template do Aggregate Root:**
```csharp
public sealed class {Entity} : BaseAuditableEntity, IAggregateRoot, IHasTenant
{
    private {Entity}() { } // EF Core

    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = default!;
    // ... outras propriedades

    public static {Entity} Create(Guid tenantId, string name /* outros params */)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var entity = new {Entity}
        {
            TenantId = tenantId,
            Name = name,
        };

        entity.Raise(new {Entity}CreatedEvent(entity.Id, DateTimeOffset.UtcNow));
        return entity;
    }

    public void Update(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        Raise(new {Entity}UpdatedEvent(Id, DateTimeOffset.UtcNow));
    }
}
```

**Template do Domain Event:**
```csharp
public sealed record {Entity}CreatedEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt) : IDomainEvent;
```

**Template do Repository Interface:**
```csharp
public interface I{Entity}Repository
{
    Task<{Entity}?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<{Entity}>> ListAsync(CancellationToken ct = default);
    Task AddAsync({Entity} entity, CancellationToken ct = default);
    Task UpdateAsync({Entity} entity, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
```

### 2. Application Layer

```
apps/backend/src/MedControl.Application/{Feature}/
├── Commands/
│   ├── Create{Entity}/
│   │   ├── Create{Entity}Command.cs
│   │   ├── Create{Entity}CommandHandler.cs
│   │   └── Create{Entity}CommandValidator.cs
│   └── Update{Entity}/
│       ├── Update{Entity}Command.cs
│       ├── Update{Entity}CommandHandler.cs
│       └── Update{Entity}CommandValidator.cs
├── Queries/
│   ├── Get{Entity}/
│   │   ├── Get{Entity}Query.cs
│   │   ├── Get{Entity}QueryHandler.cs
│   │   └── {Entity}Dto.cs
│   └── List{Entity}s/
│       ├── List{Entity}sQuery.cs
│       └── List{Entity}sQueryHandler.cs
└── EventHandlers/
    └── {Entity}CreatedEventHandler.cs
```

**Template Command:**
```csharp
public record Create{Entity}Command(string Name /* outros params */)
    : ICommand<{Entity}Dto>;

public sealed class Create{Entity}CommandHandler
    : ICommandHandler<Create{Entity}Command, {Entity}Dto>
{
    private readonly I{Entity}Repository _repository;
    private readonly ICurrentTenantService _tenant;

    public Create{Entity}CommandHandler(
        I{Entity}Repository repository,
        ICurrentTenantService tenant)
    {
        _repository = repository;
        _tenant = tenant;
    }

    public async Task<{Entity}Dto> Handle(
        Create{Entity}Command cmd, CancellationToken ct)
    {
        var entity = {Entity}.Create(_tenant.Id, cmd.Name);
        await _repository.AddAsync(entity, ct);
        return {Entity}Dto.From(entity);
    }
}
```

**Template Validator (FluentValidation):**
```csharp
public sealed class Create{Entity}CommandValidator
    : AbstractValidator<Create{Entity}Command>
{
    public Create{Entity}CommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
    }
}
```

### 3. Infrastructure Layer

```
apps/backend/src/MedControl.Infrastructure/Persistence/
├── Configurations/
│   └── {Entity}Configuration.cs
└── Repositories/
    └── {Entity}Repository.cs
```

**Template Repository:**
```csharp
internal sealed class {Entity}Repository : I{Entity}Repository
{
    private readonly ApplicationDbContext _db;
    public {Entity}Repository(ApplicationDbContext db) => _db = db;

    public async Task<{Entity}?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.{Entity}s.FindAsync([id], ct);

    public async Task<IReadOnlyList<{Entity}>> ListAsync(CancellationToken ct = default)
        => await _db.{Entity}s.ToListAsync(ct);

    public async Task AddAsync({Entity} entity, CancellationToken ct = default)
        => await _db.{Entity}s.AddAsync(entity, ct);

    public Task UpdateAsync({Entity} entity, CancellationToken ct = default)
    {
        _db.{Entity}s.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct);
        if (entity is not null) _db.{Entity}s.Remove(entity);
    }
}
```

### 4. API Layer

```
apps/backend/src/MedControl.Api/Controllers/
└── {Feature}Controller.cs
```

**Template Controller:**
```csharp
[ApiController]
[Route("api/{feature}")]
[Authorize]
public sealed class {Feature}Controller(Mediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<{Entity}Dto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        Create{Entity}Request request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new Create{Entity}Command(request.Name), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<{Entity}Dto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new Get{Entity}Query(id), ct);
        return result is null ? NotFound() : Ok(result);
    }
}
```

### 5. Tests

```
apps/backend/tests/
├── MedControl.Domain.Tests/{Feature}/
│   └── {Entity}Tests.cs
├── MedControl.Application.Tests/{Feature}/
│   ├── Create{Entity}CommandHandlerTests.cs
│   └── {Entity}ValidatorTests.cs
└── MedControl.Api.Tests/{Feature}/
    └── {Feature}ControllerTests.cs
```

---

## Lembrete: TDD First

Após criar o scaffold, use `/tdd` para implementar cada parte:
1. Comece pelos testes de domínio (`{Entity}Tests.cs`)
2. Depois application tests
3. Por último API tests

**Nunca preencha a implementação antes de ter o teste falhando.**
