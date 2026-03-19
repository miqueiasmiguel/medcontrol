using MedControl.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedControl.Infrastructure.Persistence.Configurations;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(p => p.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(p => p.DoctorId)
            .HasColumnName("doctor_id")
            .IsRequired();

        builder.Property(p => p.HealthPlanId)
            .HasColumnName("health_plan_id")
            .IsRequired();

        builder.Property(p => p.ExecutionDate)
            .HasColumnName("execution_date")
            .IsRequired();

        builder.Property(p => p.AppointmentNumber)
            .HasColumnName("appointment_number")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.AuthorizationCode)
            .HasColumnName("authorization_code")
            .HasMaxLength(100);

        builder.Property(p => p.BeneficiaryCard)
            .HasColumnName("beneficiary_card")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.BeneficiaryName)
            .HasColumnName("beneficiary_name")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(p => p.ExecutionLocation)
            .HasColumnName("execution_location")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(p => p.PaymentLocation)
            .HasColumnName("payment_location")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(p => p.Notes)
            .HasColumnName("notes");

        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Property(p => p.CreatedBy).HasColumnName("created_by");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");
        builder.Property(p => p.UpdatedBy).HasColumnName("updated_by");

        builder.Navigation(p => p.Items)
            .HasField("_items")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(p => p.Items)
            .WithOne()
            .HasForeignKey(i => i.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.TenantId)
            .HasDatabaseName("ix_payments_tenant_id");

        builder.HasIndex(p => p.DoctorId)
            .HasDatabaseName("ix_payments_doctor_id");

        builder.HasIndex(p => p.HealthPlanId)
            .HasDatabaseName("ix_payments_health_plan_id");
    }
}
