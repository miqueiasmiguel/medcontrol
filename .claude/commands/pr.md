# /pr — Criar Pull Request

Execute os passos abaixo para abrir um PR a partir do branch atual.

## 1. Verificações pré-PR

Rode os checks obrigatórios antes de abrir o PR:

```bash
# Backend
cd apps/backend
dotnet test
dotnet build --warnaserror
dotnet format --verify-no-changes

# Frontend / Monorepo (se houver mudanças)
pnpm nx affected:test
pnpm nx affected:lint
```

Se algum check falhar, corrija antes de continuar.

## 2. Commitar mudanças pendentes

Antes de abrir o PR, verifique se há mudanças não commitadas e commite seguindo o fluxo `/commit`:

```bash
git status         # checar arquivos modificados/não staged
git diff --staged  # checar o que está staged
```

Se houver mudanças pendentes:

1. Stage os arquivos relevantes:
   ```bash
   git add <arquivos>
   ```

2. Analise as mudanças e gere a mensagem no formato Conventional Commit:
   ```
   <type>(<scope>): <descrição em minúsculas, imperativo, sem ponto final>
   ```
   - Scopes disponíveis: `domain` | `app` | `infra` | `api` | `web` | `mobile` | `contracts` | `ci` | `deps` | `auth` | `tenants` | `users` | `payments` | `doctors` | `health-plans` | `procedures`
   - Descrição 100% minúscula, incluindo siglas

3. Commite:
   ```bash
   git commit -m "$(cat <<'EOF'
   <mensagem gerada>
   EOF
   )"
   ```

Repita para cada conjunto lógico de mudanças. Só avance quando `git status` não mostrar arquivos pendentes de commit.

## 3. Coletar informações do branch

```bash
git log main..HEAD --oneline        # commits que entrarão no PR
git diff main...HEAD --stat         # resumo das mudanças
```

## 4. Garantir que o branch está publicado

```bash
git push origin HEAD
```

Se o branch não existir no remoto, use `git push -u origin <nome-do-branch>`.

## 5. Redigir título e body do PR

**Título** (máx. 70 caracteres, minúsculas, imperativo):
- Seguir a mesma convenção do commit, mas em linguagem natural
- Ex.: `add ef core entity type configurations`

**Body** — use o template abaixo, preenchendo com base nos commits:

```markdown
## O que foi feito
<!-- 2–4 bullets descrevendo as mudanças principais -->

## Por que foi feito
<!-- Motivação, ticket, decisão de design -->

## Como testar
<!-- Checklist do que deve ser verificado manualmente ou via testes -->
- [ ] ...

## Checklist
- [ ] Testes passando (`dotnet test` / `pnpm nx affected:test`)
- [ ] Build sem warnings (`dotnet build --warnaserror`)
- [ ] Formato limpo (`dotnet format --verify-no-changes`)
- [ ] `/review` executado
- [ ] Nenhum secret ou dado sensível no código
```

## 6. Abrir o PR via gh

```bash
gh pr create \
  --title "<título>" \
  --body "$(cat <<'EOF'
## O que foi feito
- ...

## Por que foi feito
...

## Como testar
- [ ] ...

## Checklist
- [ ] testes passando
- [ ] build sem warnings
- [ ] formato limpo
- [ ] /review executado
EOF
)"
```

Ou de forma interativa:

```bash
gh pr create --fill   # usa commits como título/body (ponto de partida)
```

## 7. Após abrir o PR

- Copie a URL retornada e compartilhe
- Verifique se o CI está verde: `gh pr checks`
- Aguarde aprovação antes de fazer merge

## Regras deste projeto

- Branch deve ter vida curta (idealmente < 1 dia de desenvolvimento)
- PR não mistura features não relacionadas
- Merge somente com CI verde
- Base branch padrão: `main`
