using MedControl.Domain.Tenants;
using MedControl.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedControl.Infrastructure.Persistence.Configurations;

internal sealed class TenantMemberConfiguration : IEntityTypeConfiguration<TenantMember>
{
    public void Configure(EntityTypeBuilder<TenantMember> builder)
    {
        builder.ToTable("tenant_members");

        builder.HasKey(tm => tm.Id);
        builder.Property(tm => tm.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(tm => tm.TenantId)
            .HasColumnName("tenant_id");

        builder.Property(tm => tm.UserId)
            .HasColumnName("user_id");

        builder.Property(tm => tm.Role)
            .HasColumnName("role")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(tm => tm.JoinedAt)
            .HasColumnName("joined_at");

        builder.HasOne<Tenant>()
            .WithMany(nameof(Tenant.Members))
            .HasForeignKey(tm => tm.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(tm => tm.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(tm => new { tm.TenantId, tm.UserId })
            .IsUnique()
            .HasDatabaseName("ix_tenant_members_tenant_id_user_id");

        builder.HasIndex(tm => tm.UserId)
            .HasDatabaseName("ix_tenant_members_user_id");
    }
}
