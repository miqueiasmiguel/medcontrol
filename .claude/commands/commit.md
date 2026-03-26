# /commit — Commits Granulares + Merge na Main

Analise **todos** os arquivos modificados (staged e não staged) e crie commits granulares e semânticos, depois execute as verificações obrigatórias e faça merge na main.

## Processo

### 1. Diagnóstico completo

Execute em paralelo:
```bash
git status                    # arquivos staged e não staged
git diff                      # diff dos não staged
git diff --staged             # diff dos staged
git log --oneline -5          # contexto dos commits recentes
```

### 2. Agrupamento semântico

Analise o diff completo e agrupe as mudanças por contexto semântico — **nunca por camada técnica**. Exemplos de agrupamentos corretos:

- Mudanças em handler + teste + CLAUDE.md de uma mesma feature → 1 commit
- Mudança em CI + mudança em domínio → 2 commits separados
- 2 bugfixes independentes → 2 commits separados
- Arquivo de configuração (.claude/commands/) → commit `chore` separado

### 3. Para cada grupo

1. **Stage apenas os arquivos do grupo**: `git add <arquivos específicos>`
2. **Confirme o diff staged** com `git diff --staged`
3. **Crie o commit** com mensagem conventional commit
4. Repita para o próximo grupo

### 4. Verificações pré-merge (obrigatórias)

Execute **antes** do merge. Se qualquer etapa falhar, corrija antes de continuar.

#### 4a. Build e testes
```bash
# Backend
cd apps/backend && dotnet build --warnaserror && dotnet test

# Frontend / Mobile (apenas o afetado)
pnpm nx affected:lint
pnpm nx affected:test
```

#### 4b. Vulnerabilidades de pacotes
```bash
# Backend — lista pacotes com CVEs conhecidas
cd apps/backend && dotnet list package --vulnerable --include-transitive 2>&1

# JS (web + mobile) — auditoria de segurança
pnpm audit --audit-level=high 2>&1
```

**Critérios de bloqueio:**
- `dotnet list package --vulnerable` retorna qualquer pacote com severidade **High** ou **Critical** → bloqueio
- `pnpm audit` retorna vulnerabilidades com severidade **high** ou **critical** → bloqueio

> Para vulnerabilidades **moderate** ou **low**, informe o usuário mas não bloqueie o merge.
> Para pacotes transitivos sem fix disponível, documente e prossiga com aprovação explícita do usuário.

#### 4c. Formatação
```bash
cd apps/backend && dotnet format --verify-no-changes
```

### 5. Merge na main

Após todas as verificações passarem, **pedir confirmação ao usuário** antes de executar:

```bash
git log --oneline main..HEAD          # mostrar commits que serão mergeados
git checkout main
git pull origin main --rebase=false   # atualizar main antes do merge
git merge <branch> --no-ff            # merge com commit de merge explícito
git push origin main
git log --oneline -5                  # confirmar resultado
```

> Se houver conflitos no `pull` ou no `merge`, resolva-os antes de finalizar.
> Após o merge, informe o usuário sobre o resultado e se há PRs ou deploys pendentes.

---

## Formato da mensagem

```
<type>(<scope>): <descrição em minúsculas, imperativo, sem ponto final>

[corpo opcional — explica o "por quê", não o "o quê"]

[rodapé opcional — BREAKING CHANGE, closes #issue]
```

## Types

| Type | Quando usar |
|---|---|
| `feat` | Nova funcionalidade para o usuário |
| `fix` | Correção de bug |
| `test` | Adicionar/corrigir testes |
| `refactor` | Refatoração sem mudança de comportamento |
| `docs` | Documentação |
| `chore` | Build, CI, dependências, config, comandos claude |
| `perf` | Melhoria de performance |
| `style` | Formatação, espaços (sem mudança de lógica) |
| `ci` | Mudanças no CI/CD |
| `revert` | Reverter commit anterior |

## Scopes disponíveis

`domain` | `app` | `infra` | `api` | `web` | `mobile` | `contracts` | `ci` | `deps` | `auth` | `tenants` | `users` | `payments` | `doctors` | `health-plans` | `procedures` | `members`

## Regras

1. Descrição em minúsculas — sem exceções, incluindo siglas e nomes de arquivo
2. Verbo no imperativo ("add", "fix", "remove" — não "added", "fixes")
3. Sem ponto final
4. Máximo 100 caracteres na primeira linha
5. Scope é obrigatório neste projeto
6. **Nunca usar `git add .` ou `git add -A`** — sempre stage arquivos específicos
7. **Sempre pedir confirmação antes do merge na main**
8. **Nunca pular as verificações pré-merge** — vulnerabilidades high/critical bloqueiam o merge
