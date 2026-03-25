# /bugfix — Corrigir um Bug

Use este comando quando identificar um comportamento incorreto no sistema. Descreva o problema e siga o processo abaixo.

---

## Git Workflow — Obrigatório

**Antes de tocar em qualquer arquivo**, crie uma branch:

```bash
git checkout -b fix/<descricao-curta-do-bug>
# Exemplos:
# fix/payment-status-not-computed
# fix/magic-link-expired-token
# fix/doctor-profile-forbidden
```

**Após cada passo relevante**, faça um commit granular:

```bash
git commit -m "test(<scope>): add failing test reproducing <bug>"
git commit -m "fix(<scope>): <descrição da correção>"
# Se CLAUDE.md foi atualizado:
git commit -m "docs(<scope>): document <bug> pitfall in claude.md"
```

**Ao final**, pergunte ao usuário:
> "Bug corrigido. Posso fazer o merge da branch `fix/<nome>` na `main` e apagá-la?"

---

## Passo 1 — Entender o problema

Antes de tocar em qualquer arquivo, responda:

1. **Qual é o comportamento esperado?**
2. **Qual é o comportamento atual?**
3. **Como reproduzir?** (rota, payload, ação do usuário, etc.)
4. **Onde provavelmente está o problema?** (backend, frontend, infra, contrato)

Se qualquer resposta estiver incerta, leia o código relacionado antes de continuar.

---

## Passo 2 — Localizar a causa raiz

Leia os arquivos suspeitos. Procure pela causa raiz, não pelo sintoma.

```bash
# Ver o que mudou recentemente na área afetada
git log --oneline -15 -- <pasta-ou-arquivo>

# Reproduzir o erro nos testes existentes
dotnet test --filter "FullyQualifiedName~<ClasseRelacionada>"
pnpm nx test web -- --testPathPattern="<arquivo>"
```

Não corrija até ter certeza de onde está o problema.

---

## Passo 3 — Escrever um teste que reproduz o bug

Antes de corrigir, escreva um teste que **falha** pelo motivo exato do bug.

- Se já existe uma classe de testes para o código afetado, adicione o teste lá
- Se não existe, crie o arquivo de teste adequado
- O teste deve ser o mais simples possível para isolar o comportamento

```bash
# Confirmar que o teste falha pelo motivo correto (não por outro erro)
dotnet test --filter "FullyQualifiedName~<NovoTeste>"
pnpm nx test web -- --testPathPattern="<arquivo>" --testNamePattern="<nome>"
```

Se o bug é puramente visual ou de UX sem lógica testável, pule este passo e justifique.

---

## Passo 4 — Aplicar a correção mínima

Corrija apenas o necessário para o bug deixar de ocorrer.

- Não aproveite para refatorar código ao redor
- Não adicione features ou melhorias não relacionadas
- Se a correção exigir mudança em múltiplas camadas, faça camada por camada

---

## Passo 5 — Verificar

```bash
# O novo teste passa?
dotnet test --filter "FullyQualifiedName~<NovoTeste>"

# A suite inteira continua verde?
dotnet test
pnpm nx test web
```

Se algum teste não relacionado quebrou, a correção tocou em algo além do necessário — revise.

---

## Passo 6 — Documentar (se pertinente)

Avalie se o bug revela uma armadilha, convenção ausente ou decisão arquitetural que vale registrar. Se sim, adicione uma nota no(s) `CLAUDE.md` relevante(s):

| Arquivo | Quando documentar |
|---|---|
| `CLAUDE.md` (raiz) | Bug afeta convenções globais ou fluxo de feature |
| `apps/backend/CLAUDE.md` | Bug de domínio, infra, pipeline do mediator, EF Core |
| `apps/web/CLAUDE.md` | Bug de guard, rota, estado, padrão Angular |
| `apps/mobile/CLAUDE.md` | Bug de navegação, estado, padrão React Native |

O formato da nota deve ser objetivo e orientado a prevenção:

```markdown
## Armadilhas Conhecidas

### <título curto>
<!-- O que era fácil de esquecer e causou o bug -->
- **Problema**: <descrição do que estava errado>
- **Correto**: <como deve ser feito>
```

Documente apenas se a informação não estiver óbvia no código ou já coberta pelo `CLAUDE.md` existente. Pule este passo se o bug for pontual e sem lição generalizável.

