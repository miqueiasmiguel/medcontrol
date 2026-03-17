using FluentAssertions;
using MedControl.Domain.Users;
using MedControl.Infrastructure.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MedControl.Infrastructure.Tests.Persistence.Configurations;

public sealed class UserConfigurationTests(DbContextModelFixture fixture)
    : IClassFixture<DbContextModelFixture>
{
    private readonly IEntityType _et = fixture.Model.FindEntityType(typeof(User))!;

    // Ciclo 1 — Table name and PK
    [Fact]
    public void Table_ShouldBe_Users()
    {
        _et.GetTableName().Should().Be("users");
    }

    [Fact]
    public void Id_ShouldHave_CorrectColumnName()
    {
        _et.FindProperty(nameof(User.Id))!.GetColumnName().Should().Be("id");
    }

    [Fact]
    public void Id_ShouldBe_ValueGeneratedNever()
    {
        _et.FindProperty(nameof(User.Id))!.ValueGenerated.Should().Be(ValueGenerated.Never);
    }

    // Ciclo 2 — Column mappings
    [Fact]
    public void Email_ShouldHave_CorrectColumnName()
    {
        _et.FindProperty(nameof(User.Email))!.GetColumnName().Should().Be("email");
    }

    [Fact]
    public void Email_ShouldHave_MaxLength256_AndRequired()
    {
        var prop = _et.FindProperty(nameof(User.Email))!;
        prop.GetMaxLength().Should().Be(256);
        prop.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void DisplayName_ShouldHave_CorrectColumnName()
    {
        _et.FindProperty(nameof(User.DisplayName))!.GetColumnName().Should().Be("display_name");
    }

    [Fact]
    public void DisplayName_ShouldHave_MaxLength256_AndNullable()
    {
        var prop = _et.FindProperty(nameof(User.DisplayName))!;
        prop.GetMaxLength().Should().Be(256);
        prop.IsNullable.Should().BeTrue();
    }

    [Fact]
    public void IsEmailVerified_ShouldHave_CorrectColumnName()
    {
        _et.FindProperty(nameof(User.IsEmailVerified))!.GetColumnName().Should().Be("is_email_verified");
    }

    [Fact]
    public void GlobalRole_ShouldHave_CorrectColumnName()
    {
        _et.FindProperty(nameof(User.GlobalRole))!.GetColumnName().Should().Be("global_role");
    }

    [Fact]
    public void LastLoginAt_ShouldHave_CorrectColumnName_AndNullable()
    {
        var prop = _et.FindProperty(nameof(User.LastLoginAt))!;
        prop.GetColumnName().Should().Be("last_login_at");
        prop.IsNullable.Should().BeTrue();
    }

    [Fact]
    public void AuditColumns_ShouldHave_SnakeCaseNames()
    {
        _et.FindProperty("CreatedAt")!.GetColumnName().Should().Be("created_at");
        _et.FindProperty("CreatedBy")!.GetColumnName().Should().Be("created_by");
        _et.FindProperty("UpdatedAt")!.GetColumnName().Should().Be("updated_at");
        _et.FindProperty("UpdatedBy")!.GetColumnName().Should().Be("updated_by");
    }

    // Ciclo 3 — AvatarUrl Uri→string converter
    [Fact]
    public void AvatarUrl_ShouldHave_ValueConverter()
    {
        _et.FindProperty(nameof(User.AvatarUrl))!.GetValueConverter().Should().NotBeNull();
    }

    [Fact]
    public void AvatarUrl_ShouldHave_MaxLength2048_AndNullable()
    {
        var prop = _et.FindProperty(nameof(User.AvatarUrl))!;
        prop.GetMaxLength().Should().Be(2048);
        prop.IsNullable.Should().BeTrue();
    }

    // Ciclo 4 — Unique index on email
    [Fact]
    public void Email_ShouldHave_UniqueIndex()
    {
        var index = _et.GetIndexes()
            .Single(i => i.Properties.Any(p => p.Name == nameof(User.Email)));

        index.IsUnique.Should().BeTrue();
        index.GetDatabaseName().Should().Be("ix_users_email");
    }
}
