# /tdd — Test-Driven Development

Você entrou no modo TDD. Siga o ciclo **RED → GREEN → REFACTOR** estritamente.

## Git Workflow — Obrigatório

**Antes de começar**, crie uma branch com nome sugestivo:

```bash
git checkout -b <type>/<nome-descritivo>
# Exemplos:
# feat/payment-status-computed
# fix/magic-link-token-expiry
# test/doctor-profile-update
```

**Durante a implementação**, faça commits granulares a cada ciclo RED → GREEN → REFACTOR completo:

```bash
# Após RED (teste falhando)
git commit -m "test(<scope>): add failing test for <behavior>"

# Após GREEN + REFACTOR
git commit -m "feat(<scope>): implement <behavior>"
```

**Ao final de todos os ciclos**, pergunte ao usuário:
> "Todos os ciclos estão completos. Posso fazer o merge da branch `<nome>` na `main`, apagá-la e fazer push?"

---

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
❌ Avançar para o próximo ciclo com testes falhando

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

---

## 📝 PÓS-IMPLEMENTAÇÃO — Atualizar CLAUDE.md

Após todos os ciclos RED → GREEN → REFACTOR da feature, atualize os arquivos relevantes abaixo. **Só atualize o que mudou.** Informações deriváveis do código não precisam ser documentadas.

### `apps/backend/CLAUDE.md` — atualizar quando a feature tocar o backend

- Novas entidades/value objects/enums: campos, invariantes, métodos públicos relevantes
- Novos endpoints: rota, método HTTP, payload, resposta
- Novos commands/queries e seus handlers
- Novas interfaces em `Application/Common/Interfaces/`
- Novas implementações em Infrastructure (serviços, repositórios)
- Remover itens da seção "O que ainda não foi implementado" que foram concluídos

### `apps/web/CLAUDE.md` — atualizar quando a feature tocar o frontend Angular

- Novos módulos/features adicionados à estrutura de pastas
- Novas convenções de teste específicas da feature
- Novos serviços ou tokens de injeção relevantes para futuras features
- Mudanças em `app.config.ts` ou `app.routes.ts` que afetam toda a aplicação

### `apps/mobile/CLAUDE.md` — atualizar quando a feature tocar o mobile React Native

- Novas screens ou navigators adicionados
- Novos hooks ou contextos globais
- Dependências nativas adicionadas que requerem configuração extra

### `CLAUDE.md` raiz — atualizar apenas para mudanças transversais

- Mudanças de escopo ou decisões de domínio que afetam múltiplas camadas
- Novos módulos adicionados à tabela de Módulos ou Bounded Contexts
- Novos scopes válidos para commitlint
- Novas variáveis de ambiente obrigatórias

### Regra de ouro para documentação

> Só documente o que **não é óbvio ao ler o código**. Se alguém lendo o arquivo `.cs` ou `.ts` consegue derivar a informação, não precisa estar no CLAUDE.md.
