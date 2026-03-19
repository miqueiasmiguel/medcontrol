using MedControl.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedControl.Infrastructure.Persistence.Configurations;

internal sealed class PaymentItemConfiguration : IEntityTypeConfiguration<PaymentItem>
{
    public void Configure(EntityTypeBuilder<PaymentItem> builder)
    {
        builder.ToTable("payment_items");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(i => i.PaymentId)
            .HasColumnName("payment_id")
            .IsRequired();

        builder.Property(i => i.ProcedureId)
            .HasColumnName("procedure_id")
            .IsRequired();

        builder.Property(i => i.Value)
            .HasColumnName("value")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(i => i.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(i => i.Notes)
            .HasColumnName("notes");

        builder.HasIndex(i => i.PaymentId)
            .HasDatabaseName("ix_payment_items_payment_id");

        builder.HasIndex(i => i.ProcedureId)
            .HasDatabaseName("ix_payment_items_procedure_id");
    }
}
