using FluentAssertions;
using MedControl.Domain.Tenants;
using MedControl.Domain.Users;
using MedControl.Infrastructure.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MedControl.Infrastructure.Tests.Persistence.Configurations;

public sealed class TenantMemberConfigurationTests(DbContextModelFixture fixture)
    : IClassFixture<DbContextModelFixture>
{
    private readonly IEntityType _et = fixture.Model.FindEntityType(typeof(TenantMember))!;

    // Ciclo 7 — Table name, PK, columns
    [Fact]
    public void Table_ShouldBe_TenantMembers()
    {
        _et.GetTableName().Should().Be("tenant_members");
    }

    [Fact]
    public void Id_ShouldHave_CorrectColumnName_AndValueGeneratedNever()
    {
        var prop = _et.FindProperty(nameof(TenantMember.Id))!;
        prop.GetColumnName().Should().Be("id");
        prop.ValueGenerated.Should().Be(ValueGenerated.Never);
    }

    [Fact]
    public void TenantId_ShouldHave_CorrectColumnName()
    {
        _et.FindProperty(nameof(TenantMember.TenantId))!.GetColumnName().Should().Be("tenant_id");
    }

    [Fact]
    public void UserId_ShouldHave_CorrectColumnName()
    {
        _et.FindProperty(nameof(TenantMember.UserId))!.GetColumnName().Should().Be("user_id");
    }

    [Fact]
    public void Role_ShouldHave_CorrectColumnName_MaxLength_Required()
    {
        var prop = _et.FindProperty(nameof(TenantMember.Role))!;
        prop.GetColumnName().Should().Be("role");
        prop.GetMaxLength().Should().Be(100);
        prop.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void JoinedAt_ShouldHave_CorrectColumnName()
    {
        _et.FindProperty(nameof(TenantMember.JoinedAt))!.GetColumnName().Should().Be("joined_at");
    }

    // Ciclo 8 — FK delete behaviors
    [Fact]
    public void FK_ToTenant_ShouldHave_CascadeDelete()
    {
        var fk = _et.GetForeignKeys()
            .Single(f => f.PrincipalEntityType.ClrType == typeof(Tenant));

        fk.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);
    }

    [Fact]
    public void FK_ToUser_ShouldHave_RestrictDelete()
    {
        var fk = _et.GetForeignKeys()
            .Single(f => f.PrincipalEntityType.ClrType == typeof(User));

        fk.DeleteBehavior.Should().Be(DeleteBehavior.Restrict);
    }

    // Ciclo 9 — Composite unique index and user_id index
    [Fact]
    public void ShouldHave_CompositeUniqueIndex_OnTenantIdAndUserId()
    {
        var index = _et.GetIndexes()
            .Single(i => i.Properties.Count == 2 &&
                         i.Properties.Any(p => p.Name == nameof(TenantMember.TenantId)) &&
                         i.Properties.Any(p => p.Name == nameof(TenantMember.UserId)));

        index.IsUnique.Should().BeTrue();
        index.GetDatabaseName().Should().Be("ix_tenant_members_tenant_id_user_id");
    }

    [Fact]
    public void ShouldHave_SimpleIndex_OnUserId()
    {
        var index = _et.GetIndexes()
            .Single(i => i.Properties.Count == 1 &&
                         i.Properties.Single().Name == nameof(TenantMember.UserId));

        index.GetDatabaseName().Should().Be("ix_tenant_members_user_id");
    }
}
