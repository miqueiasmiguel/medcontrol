# /arch — Validação de Decisão de Arquitetura

Use quando tiver dúvida sobre onde colocar um código, como estruturar algo, ou quer validar uma decisão de design.

## Diagrama de dependências

```
┌──────────────┐
│   MedControl │
│     .Api     │ ← Controllers, Middleware, Program.cs
└──────┬───────┘
       │ depende de
┌──────▼───────┐    ┌─────────────────────┐
│   MedControl │    │     MedControl      │
│  .Application│◄───│  .Infrastructure    │
└──────┬───────┘    └─────────────────────┘
       │ depende de        implementa interfaces de Application
┌──────▼───────┐
│   MedControl │
│    .Domain   │ ← ZERO dependências externas (exceto primitivos .NET)
└──────────────┘
```

## Regra de ouro: "onde fica esse código?"

| Tipo de código | Camada |
|---|---|
| Entidades, value objects, domain events, regras de negócio | **Domain** |
| Interfaces de repositório, interfaces de serviços externos | **Domain** |
| Use cases, commands, queries, validators, behaviors | **Application** |
| Interfaces de infraestrutura (IEmailService, ICurrentUserService) | **Application** |
| EF Core, repositórios concretos, migrations | **Infrastructure** |
| Serviços externos (email, storage, OAuth) implementados | **Infrastructure** |
| Controllers, middleware, filters, serialização JSON | **Api** |
| Contratos de API compartilhados com frontend | **packages/contracts** |

## Perguntas para ajudar a decidir

**"Esse código depende de banco de dados, HTTP ou biblioteca externa?"**
→ Infrastructure

**"Esse código é uma regra de negócio que não muda independente da tecnologia?"**
→ Domain

**"Esse código orquestra um fluxo (busca dados, processa, salva)?"**
→ Application

**"Esse código lida com HTTP request/response, serialização, autenticação JWT?"**
→ Api

## Decisões arquiteturais já tomadas

### Multi-tenancy
- Global Query Filter no EF Core (não em cada repository)
- `TenantId` propagado via JWT → Middleware → `ICurrentTenantService`
- Global roles fazem bypass via `IgnoreQueryFilters()`

### Autenticação
- JWT gerado internamente (sem Identity Server, sem ASP.NET Core Identity)
- Magic Link: token one-time em IDistributedCache (TTL 15min)
- Google OAuth: troca de code → user info → criar/atualizar User local
- Refresh token: GUID rotativo armazenado em cache

### Mediator
- Customizado (sem MediatR) — resolve handlers via IServiceProvider
- Pipeline: LoggingBehavior → ValidationBehavior → TransactionBehavior → Handler
- Commands com side-effects SEMPRE passam por TransactionBehavior

### Domain Events
- Armazenados em memória na entidade durante a request
- Despachados no SaveChanges via `DomainEventDispatchInterceptor`
- Handlers de eventos são Application services (registrados no DI)

### Testes
- Unit (Domain): sem mocks, testa comportamento puro
- Unit (Application): mocks de repositórios com NSubstitute
- Integration (Infrastructure): Testcontainers com Postgres real
- Functional (Api): WebApplicationFactory, banco em memória ou Testcontainers
- Architecture: NetArchTest valida regras de dependência

## ADR — Como registrar decisões

Se tomar uma decisão arquitetural significativa, registrar em:
`docs/adr/YYYY-MM-DD-titulo.md`

Formato simples:
```markdown
# ADR: [Título]
**Data:** YYYY-MM-DD
**Status:** Aceito

## Contexto
Por que essa decisão foi necessária?

## Decisão
O que foi decidido?

## Consequências
Quais os trade-offs?
```
