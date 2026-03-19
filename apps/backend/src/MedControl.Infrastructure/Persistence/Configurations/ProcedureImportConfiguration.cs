using MedControl.Domain.Procedures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedControl.Infrastructure.Persistence.Configurations;

internal sealed class ProcedureImportConfiguration : IEntityTypeConfiguration<ProcedureImport>
{
    public void Configure(EntityTypeBuilder<ProcedureImport> builder)
    {
        builder.ToTable("procedure_imports");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(i => i.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(i => i.Source)
            .HasColumnName("source")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(i => i.EffectiveFrom)
            .HasColumnName("effective_from")
            .IsRequired();

        builder.Property(i => i.TotalRows)
            .HasColumnName("total_rows")
            .IsRequired();

        builder.Property(i => i.ImportedRows)
            .HasColumnName("imported_rows")
            .IsRequired();

        builder.Property(i => i.SkippedRows)
            .HasColumnName("skipped_rows")
            .IsRequired();

        builder.Property(i => i.ErrorSummary)
            .HasColumnName("error_summary")
            .HasMaxLength(2000);

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(i => i.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(i => i.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(i => i.UpdatedBy)
            .HasColumnName("updated_by");

        builder.HasIndex(i => i.TenantId)
            .HasDatabaseName("ix_procedure_imports_tenant_id");
    }
}
