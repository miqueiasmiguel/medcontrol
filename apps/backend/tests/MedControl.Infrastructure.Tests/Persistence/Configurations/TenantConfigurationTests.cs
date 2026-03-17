using FluentAssertions;
using MedControl.Domain.Tenants;
using MedControl.Infrastructure.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MedControl.Infrastructure.Tests.Persistence.Configurations;

public sealed class TenantConfigurationTests(DbContextModelFixture fixture)
    : IClassFixture<DbContextModelFixture>
{
    private readonly IEntityType _et = fixture.Model.FindEntityType(typeof(Tenant))!;

    // Ciclo 5 — Table name, PK, columns and slug index
    [Fact]
    public void Table_ShouldBe_Tenants()
    {
        _et.GetTableName().Should().Be("tenants");
    }

    [Fact]
    public void Id_ShouldHave_CorrectColumnName_AndValueGeneratedNever()
    {
        var prop = _et.FindProperty(nameof(Tenant.Id))!;
        prop.GetColumnName().Should().Be("id");
        prop.ValueGenerated.Should().Be(ValueGenerated.Never);
    }

    [Fact]
    public void Name_ShouldHave_CorrectColumnName_MaxLength_Required()
    {
        var prop = _et.FindProperty(nameof(Tenant.Name))!;
        prop.GetColumnName().Should().Be("name");
        prop.GetMaxLength().Should().Be(256);
        prop.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void Slug_ShouldHave_CorrectColumnName_MaxLength_Required()
    {
        var prop = _et.FindProperty(nameof(Tenant.Slug))!;
        prop.GetColumnName().Should().Be("slug");
        prop.GetMaxLength().Should().Be(256);
        prop.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void IsActive_ShouldHave_CorrectColumnName()
    {
        _et.FindProperty(nameof(Tenant.IsActive))!.GetColumnName().Should().Be("is_active");
    }

    [Fact]
    public void AuditColumns_ShouldHave_SnakeCaseNames()
    {
        _et.FindProperty("CreatedAt")!.GetColumnName().Should().Be("created_at");
        _et.FindProperty("CreatedBy")!.GetColumnName().Should().Be("created_by");
        _et.FindProperty("UpdatedAt")!.GetColumnName().Should().Be("updated_at");
        _et.FindProperty("UpdatedBy")!.GetColumnName().Should().Be("updated_by");
    }

    [Fact]
    public void Slug_ShouldHave_UniqueIndex()
    {
        var index = _et.GetIndexes()
            .Single(i => i.Properties.Any(p => p.Name == nameof(Tenant.Slug)));

        index.IsUnique.Should().BeTrue();
        index.GetDatabaseName().Should().Be("ix_tenants_slug");
    }

    // Ciclo 6 — Members navigation backing field
    [Fact]
    public void Members_Navigation_ShouldUse_BackingField()
    {
        var nav = _et.FindNavigation(nameof(Tenant.Members))!;
        nav.GetFieldName().Should().Be("_members");
    }
}
