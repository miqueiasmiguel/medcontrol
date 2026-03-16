# /review — Code Review Checklist

Execute este checklist antes de abrir qualquer PR. Leia os arquivos modificados e verifique cada item.

## 1. Arquitetura & Clean Architecture

- [ ] Dependências respeitam a direção: Domain ← Application ← Infrastructure / API
- [ ] Domain não referencia Application, Infrastructure ou qualquer pacote externo
- [ ] Controllers não contêm lógica de negócio (delegam ao Mediator)
- [ ] Repositórios são interfaces no Domain, implementados na Infrastructure
- [ ] Sem `new DbContext()` fora de Infrastructure

## 2. DDD & Domínio Rico

- [ ] Entidades têm private/protected setters (não public)
- [ ] Lógica de negócio está nos métodos da entidade, não no handler
- [ ] Factory methods com validação (ex: `Tenant.Create(...)`)
- [ ] Domain events são levantados corretamente com `Raise()`
- [ ] Value objects são imutáveis (record ou readonly properties)
- [ ] Invariantes do domínio são protegidas (lança exceção se violada)

## 3. Multi-Tenancy

- [ ] Todas as entidades tenant-scoped implementam `IHasTenant`
- [ ] Nenhuma query busca dados sem global query filter (ou intencional com `IgnoreQueryFilters`)
- [ ] Não há `TenantId` hardcoded
- [ ] Bypass de tenant (global roles) está explícito e justificado

## 4. Mediator & Behaviors

- [ ] Commands/Queries implementam `ICommand<T>` ou `IQuery<T>` corretamente
- [ ] Handlers são `sealed` e tem um único construtor
- [ ] Validators implementam `AbstractValidator<TCommand>`
- [ ] Todos os commands com side-effects passam pelo `TransactionBehavior`

## 5. Qualidade de Código .NET

- [ ] `async/await` consistente (sem `.Result` ou `.Wait()`)
- [ ] `CancellationToken` propagado em todos os métodos async
- [ ] Sem `ArgumentNullException` manual (use `ArgumentNullException.ThrowIfNull`)
- [ ] Records para DTOs (imutáveis, sem lógica)
- [ ] `sealed` em classes que não serão herdadas
- [ ] Sem `string.IsNullOrEmpty` manual (use `ArgumentException.ThrowIfNullOrWhiteSpace`)

## 6. Testes

- [ ] Todo código novo tem testes (TDD foi seguido?)
- [ ] Testes de domínio não usam mocks (testam comportamento puro)
- [ ] Testes de handler usam NSubstitute para repositórios
- [ ] Testes de integração usam Testcontainers (não SQLite para simular Postgres)
- [ ] Testes de API usam `WebApplicationFactory`
- [ ] Nomes de teste: `MetodoOuBehavior_Condicao_Resultado`
- [ ] Sem testes com `Thread.Sleep` ou datas hardcoded

## 7. Segurança

- [ ] Endpoints autenticados têm `[Authorize]`
- [ ] Sem secrets/senhas no código ou logs
- [ ] Inputs validados no validator (não no handler)
- [ ] Sem SQL concatenado manualmente (use EF Core ou parâmetros)
- [ ] CORS configurado corretamente (não `AllowAnyOrigin` em produção)

## 8. Performance

- [ ] Sem N+1 queries (use `.Include()` ou projections)
- [ ] Queries grandes usam paginação
- [ ] Sem `ToList()` desnecessário antes de filtrar
- [ ] Cache usado onde faz sentido

## 9. Conventional Commits & PR

- [ ] Título do PR é claro e descreve o que foi feito
- [ ] Todos os commits seguem conventional commits
- [ ] Branch tem vida curta (< 1 dia de desenvolvimento)
- [ ] PR não mistura múltiplas features não relacionadas

## 10. TypeScript/Angular (se aplicável)

- [ ] Sem `any` no TypeScript
- [ ] Sem `subscribe` sem `takeUntilDestroyed` ou `async pipe`
- [ ] Componentes usam `OnPush` change detection
- [ ] Sem lógica de negócio em componentes (delegue para services)
- [ ] Contratos tipados com `@medcontrol/contracts`

---

## Resultado

Se todos os itens estão marcados: **PR pronto para abrir** ✅

Se algum item falhou: corrija antes de abrir o PR.
