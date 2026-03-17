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

## 2. Coletar informações do branch

```bash
git status                          # arquivos pendentes de commit
git log main..HEAD --oneline        # commits que entrarão no PR
git diff main...HEAD --stat         # resumo das mudanças
```

## 3. Garantir que o branch está publicado

```bash
git push origin HEAD
```

Se o branch não existir no remoto, use `git push -u origin <nome-do-branch>`.

## 4. Redigir título e body do PR

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

## 5. Abrir o PR via gh

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

## 6. Após abrir o PR

- Copie a URL retornada e compartilhe
- Verifique se o CI está verde: `gh pr checks`
- Aguarde aprovação antes de fazer merge

## Regras deste projeto

- Branch deve ter vida curta (idealmente < 1 dia de desenvolvimento)
- PR não mistura features não relacionadas
- Merge somente com CI verde
- Base branch padrão: `main`
