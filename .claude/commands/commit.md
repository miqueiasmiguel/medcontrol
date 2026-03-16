# /commit — Gerar Mensagem Conventional Commit

Analise os arquivos modificados (use `git diff --staged` ou `git status`) e sugira a mensagem de commit correta.

## Formato

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
| `chore` | Build, CI, dependências, config |
| `perf` | Melhoria de performance |
| `style` | Formatação, espaços (sem mudança de lógica) |
| `ci` | Mudanças no CI/CD |
| `revert` | Reverter commit anterior |

## Scopes disponíveis

`domain` | `app` | `infra` | `api` | `web` | `mobile` | `contracts` | `ci` | `deps` | `auth` | `tenants` | `users`

## Exemplos

```bash
# Nova feature
feat(auth): add magic link token generation

# Correção de bug
fix(domain): prevent duplicate tenant creation for same email

# Testes
test(app): add unit tests for send magic link command handler

# Refatoração
refactor(infra): extract token validation to dedicated service

# CI
ci: add code coverage threshold enforcement to backend pipeline

# Breaking change
feat(api)!: change authentication endpoint from /login to /auth/token

BREAKING CHANGE: /login endpoint removed, use /auth/token instead
```

## Regras

1. Descrição em minúsculas
2. Verbo no imperativo ("add", "fix", "remove" — não "added", "fixes")
3. Sem ponto final
4. Máximo 100 caracteres na primeira linha
5. Scope é obrigatório neste projeto

## Verificar antes de commitar

```bash
git diff --staged  # ver o que está staged
git status         # ver arquivos não staged
pnpm nx affected:test  # garantir que testes passam
pnpm nx affected:lint  # garantir lint limpo
```
