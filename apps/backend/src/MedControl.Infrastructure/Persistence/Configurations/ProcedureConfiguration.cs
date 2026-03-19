using MedControl.Domain.Procedures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedControl.Infrastructure.Persistence.Configurations;

internal sealed class ProcedureConfiguration : IEntityTypeConfiguration<Procedure>
{
    public void Configure(EntityTypeBuilder<Procedure> builder)
    {
        builder.ToTable("procedures");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(p => p.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(p => p.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(p => p.Value)
            .HasColumnName("value")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(p => p.EffectiveFrom)
            .HasColumnName("effective_from")
            .IsRequired();

        builder.Property(p => p.EffectiveTo)
            .HasColumnName("effective_to");

        builder.Property(p => p.Source)
            .HasColumnName("source")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(p => p.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(p => p.UpdatedBy)
            .HasColumnName("updated_by");

        builder.HasIndex(p => new { p.TenantId, p.Code, p.EffectiveFrom })
            .IsUnique()
            .HasDatabaseName("ix_procedures_tenant_code_effective_from");

        builder.HasIndex(p => new { p.TenantId, p.EffectiveFrom, p.EffectiveTo })
            .HasDatabaseName("ix_procedures_tenant_effective_dates");

        builder.HasIndex(p => p.TenantId)
            .HasDatabaseName("ix_procedures_tenant_id");
    }
}
