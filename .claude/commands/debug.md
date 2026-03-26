# /debug — Diagnóstico Sistemático

Use quando um teste falha, build quebra ou comportamento inesperado aparece.

## Passo 1 — Reproduzir o problema

Antes de qualquer coisa, confirme:
- O erro é consistente ou intermitente?
- Em qual contexto ocorre (local, CI, produção)?
- O que mudou recentemente (git log)?

```bash
git log --oneline -10
git diff HEAD~1
```

## Passo 2 — Ler o erro completamente

Não tente adivinhar. Leia a mensagem de erro inteira:

```bash
# .NET — mais verboso
dotnet test --logger "console;verbosity=detailed" 2>&1 | head -100

# Build com diagnóstico
dotnet build --verbosity diagnostic 2>&1 | grep -E "(error|warning|Error|Warning)"

# Angular
pnpm nx test web -- --verbose 2>&1 | tail -50
```

## Passo 3 — Isolar o problema

```bash
# .NET — rodar apenas o teste que falha
dotnet test --filter "FullyQualifiedName~NomeDaClasse.NomeDoTeste"

# Verificar se é problema de configuração
dotnet test --filter "Category=Unit"  # isola unitários
dotnet test --filter "Category=Integration"  # isola integração
```

## Passo 4 — Hipótese → Teste → Verificação

Formule uma hipótese:
> "O problema é X porque Y"

Escreva um teste que reproduza o problema:
```csharp
[Fact]
public void Reproduz_OProblema()
{
    // Arrange — mínimo necessário para reproduzir
    // Act
    // Assert — o que deveria acontecer
}
```

Verifique se o teste falha pelo motivo correto (não por outro motivo).

## Passo 5 — Problemas comuns por categoria

### Build falha com warnings como erros
```bash
# Ver qual warning está causando
dotnet build 2>&1 | grep "warning"
# Corrigir o warning, não suprimir com #pragma
```

### Teste de integração falha (Testcontainers)
```bash
# Docker rodando?
docker ps
# Versão da imagem correta?
# Timeout muito curto?
```

### Erro de multi-tenancy (dados de outro tenant aparecendo)
```csharp
// Verificar se IHasTenant está implementado
// Verificar se global query filter está configurado no DbContext
// Verificar se TenantResolutionMiddleware está registrado
```

### EF Core migration falha
```bash
dotnet ef migrations list --project src/MedControl.Infrastructure \
  --startup-project src/MedControl.Api

# Remover migration problemática
dotnet ef migrations remove --project src/MedControl.Infrastructure \
  --startup-project src/MedControl.Api
```

### JWT/Autenticação falhando
```csharp
// Verificar: JWT_SECRET tem >= 256 bits?
// Verificar: issuer e audience batem?
// Verificar: token não expirou?
// Use jwt.io para decodificar o token
```

### Mediator não encontra handler
```csharp
// Handler está registrado no DI?
// MediatorExtensions.AddHandlers foi chamado no Program.cs?
// Namespace correto?
```

## Passo 6 — Fix e verificação

1. Aplique a correção mínima
2. Execute o teste que reproduz o problema → deve passar
3. Execute toda a suite → nada deve quebrar

---

## Git Workflow — Quando o debug resultar em código alterado

Se o diagnóstico exigiu mudanças em código (não apenas leitura), crie uma branch antes de aplicar o fix:

```bash
git checkout -b fix/<descricao-do-problema>
# Exemplos:
# fix/testcontainers-timeout
# fix/jwt-issuer-mismatch
# fix/mediator-handler-not-registered
```

Após corrigir e verificar, execute `/commit` para criar os commits, rodar as verificações pré-merge (build, testes, vulnerabilidades) e fazer o merge na main.
