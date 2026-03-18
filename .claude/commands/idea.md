# /idea — Avaliação de Feature ou Caso de Uso

Use quando quiser validar uma ideia, explorar um fluxo, ou entender os trade-offs antes de investir em implementação.

## O que fazer ao receber uma ideia

O usuário vai apresentar uma feature, caso de uso, ou decisão de produto/técnica. Sua tarefa é:

### 1. Entender e reformular

Reformule a ideia com suas próprias palavras em 2-3 linhas para confirmar o entendimento. Se faltar contexto essencial, faça no máximo 2 perguntas diretas antes de continuar.

### 2. Fluxo sugerido

Descreva o fluxo principal (happy path) passo a passo. Use diagrama ASCII ou lista numerada conforme fizer mais sentido.

Se for uma feature com UI, inclua o fluxo do ponto de vista do usuário (telas, ações, feedback).
Se for backend, inclua o fluxo técnico: request → camada → persistência → resposta.

### 3. Mapeamento arquitetural

Para cada parte do fluxo, identifique onde o código vai viver segundo a arquitetura do projeto:

| Parte | Camada | Motivo |
|---|---|---|
| (ex: validar CRM único) | Domain | Regra de negócio pura |
| (ex: orquestrar criação) | Application | Coordena repositório + evento |
| (ex: persistir DoctorProfile) | Infrastructure | EF Core |
| (ex: receber request HTTP) | Api | Controller |

### 4. Prós e Contras

Liste os pontos positivos e negativos da abordagem sugerida — seja honesto sobre complexidade, acoplamento, manutenibilidade e impacto no time.

**Prós:**
- ...

**Contras / Riscos:**
- ...

### 5. Avaliação da ideia do usuário

Se o usuário apresentou uma abordagem ou decisão específica, avalie diretamente:

- O que está bem pensado
- O que pode ser problemático (com motivo claro)
- Sugestão de ajuste se necessário

Seja direto. Não valide ideia ruim por educação — aponte o problema e ofereça alternativa.

### 6. Alternativas (se relevante)

Se houver abordagem alternativa que valha considerar, apresente brevemente com o principal trade-off de cada uma.

### 7. Próximo passo sugerido

Termine com uma recomendação clara de próximo passo:
- Usar `/tdd` para começar a implementação
- Usar `/arch` se a dúvida arquitetural ainda não está resolvida
- Prototipar primeiro (e o que validar)
- Levantar mais requisitos (e quais)

---

## Contexto do projeto

Stack: .NET 10 + Angular + React Native + PostgreSQL + EF Core
Arquitetura: Clean Architecture + DDD + Result Pattern + Mediator customizado
Multi-tenancy via global query filter. JWT com claims `sub`, `email`, `tenant_id`, `roles`.
Bounded contexts: Payments, Doctors, HealthPlans, Procedures.
