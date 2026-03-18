using MedControl.Domain.HealthPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedControl.Infrastructure.Persistence.Configurations;

internal sealed class HealthPlanConfiguration : IEntityTypeConfiguration<HealthPlan>
{
    public void Configure(EntityTypeBuilder<HealthPlan> builder)
    {
        builder.ToTable("health_plans");

        builder.HasKey(hp => hp.Id);
        builder.Property(hp => hp.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(hp => hp.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(hp => hp.Name)
            .HasColumnName("name")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(hp => hp.TissCode)
            .HasColumnName("tiss_code")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(hp => hp.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(hp => hp.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(hp => hp.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(hp => hp.UpdatedBy)
            .HasColumnName("updated_by");

        builder.HasIndex(hp => new { hp.TenantId, hp.TissCode })
            .IsUnique()
            .HasDatabaseName("ix_health_plans_tenant_tiss_code");

        builder.HasIndex(hp => hp.TenantId)
            .HasDatabaseName("ix_health_plans_tenant_id");
    }
}
