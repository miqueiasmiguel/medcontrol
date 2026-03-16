## O que foi feito

<!-- Descreva o que foi implementado e por quê. Se houver issue relacionada, referencie: Closes #123 -->

## Tipo de mudança

- [ ] `feat` — nova funcionalidade
- [ ] `fix` — correção de bug
- [ ] `refactor` — refatoração sem mudança de comportamento
- [ ] `test` — adição/correção de testes
- [ ] `chore` — build, CI, dependências, config
- [ ] `docs` — documentação

## Checklist

### Qualidade

- [ ] Testes passando (`dotnet test` / `pnpm nx test`)
- [ ] Build sem warnings (`--warnaserror`)
- [ ] Lint limpo (`dotnet format --verify-no-changes`)
- [ ] Cobertura >= threshold (80% backend, 75% web, 70% mobile)

### Arquitetura (backend)

- [ ] Domain não depende de Application/Infrastructure/Api
- [ ] Entidades tenant-scoped implementam `IHasTenant`
- [ ] Nenhum bypass de tenant sem justificativa explícita
- [ ] Sem lógica de negócio em controllers ou handlers (está nas entidades)
- [ ] Commands/Queries com nomes claros no padrão `VerbNounCommand/Query`

### Segurança

- [ ] Nenhum secret no código
- [ ] Endpoints que precisam de auth têm `[Authorize]`
- [ ] Inputs validados via FluentValidation

### TDD

- [ ] Ciclo RED → GREEN → REFACTOR foi seguido?
- [ ] Testes testam comportamento, não implementação interna

## Screenshot / evidência (se aplicável)

<!-- Adicione screenshots ou logs relevantes -->
