# /tdd — Test-Driven Development

Você entrou no modo TDD. Siga o ciclo **RED → GREEN → REFACTOR → COMMIT** estritamente.

## Regra de ouro

**Nunca escreva código de produção sem um teste falhando primeiro.**

Se o usuário pedir para implementar algo sem mencionar testes, pergunte:
> "Qual comportamento queremos testar primeiro?"

---

## Ciclo Completo

### 🔴 RED — Escrever o teste que falha

1. Identifique o **menor comportamento** a implementar
2. Escreva **um único teste** que descreve esse comportamento
3. Execute os testes e confirme que **falha pelo motivo correto**
4. Não escreva nada mais ainda

**Templates por camada:**

```csharp
// Domain (entidade/value object)
[Fact]
public void NomeDoBehavior_Condicao_DeveResultar()
{
    // Arrange
    // Act
    // Assert — use FluentAssertions
    result.Should().Be(expected);
}

// Application (handler)
[Fact]
public async Task Handle_QuandoCondicao_DeveResultar()
{
    // Arrange — mock repositórios com NSubstitute
    var repo = Substitute.For<IRepository>();
    var handler = new Handler(repo);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    await repo.Received(1).AddAsync(Arg.Any<Entity>(), Arg.Any<CancellationToken>());
}

// API (endpoint)
[Fact]
public async Task POST_Endpoint_QuandoValido_DeveRetornar201()
{
    // Arrange — WebApplicationFactory
    var response = await _client.PostAsJsonAsync("/api/endpoint", payload);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
}
```

### 🟢 GREEN — Código mínimo para passar

1. Escreva o **mínimo absoluto** para o teste passar
2. Não adicione features extras, não antecipe necessidades futuras
3. Pode ser feio, pode ser simples — só precisa passar
4. Execute os testes: todos devem estar verdes

### 🔵 REFACTOR — Limpar sem quebrar

1. **Todos os testes passando** antes de refatorar
2. Remova duplicação
3. Melhore nomes
4. Extraia método/classe se necessário
5. Execute testes após cada mudança
6. **Nunca refatore código de teste e de produção ao mesmo tempo**

### ✅ COMMIT — Fechar o ciclo

```bash
git add <arquivos-específicos>
git commit -m "feat(scope): descrição em minúsculas"
# Exemplo: "feat(domain): add tenant creation with domain event"
# Exemplo: "test(app): add unit tests for magic link handler"
```

---

## Comandos para executar testes

```bash
# Backend — .NET
cd apps/backend
dotnet test                                    # todos
dotnet test --filter "FullyQualifiedName~NomeTeste"  # filtrado
dotnet test --watch                            # watch mode

# Cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Frontend — Angular
pnpm nx test web
pnpm nx test web --watch

# Mobile — React Native
pnpm nx test mobile
```

---

## Anti-padrões — NUNCA FAÇA

❌ Implementar antes de escrever o teste
❌ Escrever múltiplos testes de uma vez
❌ Testar detalhes de implementação (testar comportamento, não como)
❌ Testes que dependem de outros testes
❌ Mockar o que você não precisa
❌ Refatorar com testes falhando
❌ Pular o commit após refactor

---

## Exemplo completo — Magic Link Command

**Passo 1 — RED:**
```csharp
[Fact]
public async Task Handle_QuandoEmailValido_DeveSalvarTokenEEnviarEmail()
{
    var emailService = Substitute.For<IEmailService>();
    var userRepo = Substitute.For<IUserRepository>();
    var tokenService = Substitute.For<IMagicLinkService>();
    tokenService.GenerateToken(Arg.Any<Guid>()).Returns("token-123");

    var handler = new SendMagicLinkCommandHandler(userRepo, tokenService, emailService);
    await handler.Handle(new SendMagicLinkCommand("user@test.com"), CancellationToken.None);

    await emailService.Received(1).SendAsync(
        "user@test.com",
        Arg.Any<string>(),
        Arg.Is<string>(s => s.Contains("token-123")),
        Arg.Any<CancellationToken>());
}
```
→ Executar: vermelha ✓

**Passo 2 — GREEN:**
```csharp
public async Task Handle(SendMagicLinkCommand cmd, CancellationToken ct)
{
    var user = await _userRepo.GetByEmailAsync(cmd.Email, ct) ?? User.Create(cmd.Email);
    var token = _tokenService.GenerateToken(user.Id);
    await _emailService.SendAsync(cmd.Email, "Seu link", $"Token: {token}", ct);
}
```
→ Executar: verde ✓

**Passo 3 — REFACTOR:**
Extrair template do email para constante, melhorar mensagem, etc.
→ Executar: ainda verde ✓

**Passo 4 — COMMIT:**
`feat(auth): add send magic link command with email notification`

---

## 📝 PÓS-IMPLEMENTAÇÃO — Atualizar CLAUDE.md

Após todos os ciclos RED → GREEN → REFACTOR → COMMIT da feature:

1. Atualizar `apps/backend/CLAUDE.md` com:
   - Novas entidades/enums criados (estrutura de campos, comportamentos)
   - Novos endpoints adicionados
   - Novos serviços ou interfaces de Application/Infrastructure
   - Atualizar a seção "O que ainda não foi implementado" removendo o que foi feito

2. Atualizar `CLAUDE.md` raiz (se pertinente) com:
   - Mudanças de escopo ou decisões de domínio relevantes
   - Novos scopes de commit (se adicionados)

3. Apenas informações que **não são deriváveis do código** — não duplicar o que já está no código fonte.
