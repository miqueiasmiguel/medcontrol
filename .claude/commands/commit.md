# /commit — Commits Granulares + Merge na Main

Analise **todos** os arquivos modificados (staged e não staged) e crie commits granulares e semânticos, depois tente merge na main.

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

### 4. Merge na main

Após todos os commits:
```bash
git log --oneline main..HEAD  # mostrar commits que serão mergeados
git checkout main
git merge <branch> --no-ff    # merge com commit de merge explícito
git log --oneline -5          # confirmar resultado
```

> Se houver conflitos, resolva-os antes de finalizar o merge.

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
