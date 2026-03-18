using MedControl.Domain.Doctors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedControl.Infrastructure.Persistence.Configurations;

internal sealed class DoctorProfileConfiguration : IEntityTypeConfiguration<DoctorProfile>
{
    public void Configure(EntityTypeBuilder<DoctorProfile> builder)
    {
        builder.ToTable("doctor_profiles");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(d => d.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(d => d.UserId)
            .HasColumnName("user_id");

        builder.Property(d => d.Name)
            .HasColumnName("name")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(d => d.Crm)
            .HasColumnName("crm")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(d => d.CouncilState)
            .HasColumnName("council_state")
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(d => d.Specialty)
            .HasColumnName("specialty")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(d => d.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(d => d.UpdatedBy)
            .HasColumnName("updated_by");

        builder.HasIndex(d => new { d.TenantId, d.Crm, d.CouncilState })
            .IsUnique()
            .HasDatabaseName("ix_doctor_profiles_tenant_crm_state");

        builder.HasIndex(d => d.TenantId)
            .HasDatabaseName("ix_doctor_profiles_tenant_id");
    }
}
