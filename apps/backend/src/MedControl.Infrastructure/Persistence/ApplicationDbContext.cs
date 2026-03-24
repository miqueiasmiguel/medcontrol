using MedControl.Application.Common.Interfaces;
using MedControl.Domain.Common;
using MedControl.Domain.Doctors;
using MedControl.Domain.HealthPlans;
using MedControl.Domain.Payments;
using MedControl.Domain.Procedures;
using MedControl.Domain.Tenants;
using MedControl.Domain.Users;
using MedControl.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace MedControl.Infrastructure.Persistence;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    AuditableEntityInterceptor auditInterceptor,
    DomainEventDispatchInterceptor domainEventInterceptor,
    ICurrentUserService currentUser)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantMember> TenantMembers => Set<TenantMember>();
    public DbSet<User> Users => Set<User>();
    public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();
    public DbSet<HealthPlan> HealthPlans => Set<HealthPlan>();
    public DbSet<Procedure> Procedures => Set<Procedure>();
    public DbSet<ProcedureImport> ProcedureImports => Set<ProcedureImport>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .AddInterceptors(auditInterceptor, domainEventInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filters for multi-tenancy
        modelBuilder.Entity<TenantMember>()
            .HasQueryFilter(m => currentUser.TenantId.HasValue && m.TenantId == currentUser.TenantId.Value);

        modelBuilder.Entity<DoctorProfile>()
            .HasQueryFilter(d => currentUser.TenantId.HasValue && d.TenantId == currentUser.TenantId.Value);

        modelBuilder.Entity<HealthPlan>()
            .HasQueryFilter(hp => currentUser.TenantId.HasValue && hp.TenantId == currentUser.TenantId.Value);

        modelBuilder.Entity<Procedure>()
            .HasQueryFilter(p => currentUser.TenantId.HasValue && p.TenantId == currentUser.TenantId.Value);

        modelBuilder.Entity<ProcedureImport>()
            .HasQueryFilter(i => currentUser.TenantId.HasValue && i.TenantId == currentUser.TenantId.Value);

        modelBuilder.Entity<Payment>()
            .HasQueryFilter(p => currentUser.TenantId.HasValue && p.TenantId == currentUser.TenantId.Value);

        base.OnModelCreating(modelBuilder);
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (Database.CurrentTransaction is null)
        {
            await Database.BeginTransactionAsync(ct);
        }
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (Database.CurrentTransaction is not null)
        {
            await Database.CommitTransactionAsync(ct);
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (Database.CurrentTransaction is not null)
        {
            await Database.RollbackTransactionAsync(ct);
        }
    }
}
