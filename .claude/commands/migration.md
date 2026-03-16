# /migration — EF Core Migration

## Criar nova migration

```bash
cd apps/backend

# Criar migration
dotnet ef migrations add <NomeDaMigration> \
  --project src/MedControl.Infrastructure \
  --startup-project src/MedControl.Api \
  --output-dir Persistence/Migrations

# Exemplos de nomes:
# InitialCreate
# AddTenantMembership
# AddUserGlobalRole
# AddAuthProviderToUser
```

## Aplicar migrations

```bash
# Desenvolvimento local
dotnet ef database update \
  --project src/MedControl.Infrastructure \
  --startup-project src/MedControl.Api

# Verificar migrations pendentes
dotnet ef migrations list \
  --project src/MedControl.Infrastructure \
  --startup-project src/MedControl.Api
```

## Remover última migration (se não foi aplicada)

```bash
dotnet ef migrations remove \
  --project src/MedControl.Infrastructure \
  --startup-project src/MedControl.Api
```

## Checklist antes de criar migration

- [ ] Novas entidades implementam `IHasTenant` (se tenant-scoped)?
- [ ] Configuração `IEntityTypeConfiguration<T>` foi criada?
- [ ] DbSet foi adicionado ao `ApplicationDbContext`?
- [ ] Global query filter foi adicionado para entidades tenant-scoped?
- [ ] Migration gerada é reversível (tem `Down()` funcional)?

## Checar migration gerada

Sempre revisar o arquivo `.cs` gerado em `Persistence/Migrations/`:

```csharp
// ✅ Migration deve ter Up() e Down() completos
// ✅ Verificar tipos de coluna (varchar vs text, decimal precision)
// ✅ Verificar índices e foreign keys
// ✅ Verificar nullable vs not null
// ❌ Não modificar migrations já aplicadas em produção
```

## Commit da migration

```bash
git add apps/backend/src/MedControl.Infrastructure/Persistence/Migrations/
git commit -m "chore(infra): add migration <NomeDaMigration>"
```
