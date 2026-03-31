using FluentAssertions;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;

namespace MedControl.Domain.Tests.Tenants;

public class TenantCreateTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldReturnFailure(string? name)
    {
        var result = Tenant.Create(name!);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Tenant.Errors.NameRequired);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Create_WithValidName_ShouldReturnSuccessWithTenant()
    {
        var result = Tenant.Create("Clínica Saúde");

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Clínica Saúde");
        result.Value.Slug.Should().Be("clínica-saúde");
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithValidName_ShouldRaiseTenantCreatedEvent()
    {
        var result = Tenant.Create("Clínica Saúde");

        result.Value.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<MedControl.Domain.Tenants.Events.TenantCreatedEvent>();
    }
}

public class TenantUpdateTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_ShouldReturnFailure(string? name)
    {
        var tenant = Tenant.Create("Original").Value;

        var result = tenant.Update(name!);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Tenant.Errors.NameRequired);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Update_WithValidName_ShouldReturnSuccessAndMutateName()
    {
        var tenant = Tenant.Create("Original").Value;

        var result = tenant.Update("  Novo Nome  ");

        result.IsSuccess.Should().BeTrue();
        tenant.Name.Should().Be("Novo Nome");
    }
}

public class TenantAddMemberTests
{
    [Fact]
    public void AddMember_WhenUserAlreadyMember_ShouldReturnFailure()
    {
        var tenant = Tenant.Create("Clínica").Value;
        var userId = Guid.NewGuid();
        tenant.AddMember(userId, TenantRole.Admin);

        var result = tenant.AddMember(userId, TenantRole.Admin);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Tenant.Errors.MemberAlreadyExists);
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void AddMember_WithOperatorRole_ShouldSetRoleCorrectly()
    {
        var tenant = Tenant.Create("Clinic").Value;
        var userId = Guid.NewGuid();

        var result = tenant.AddMember(userId, TenantRole.Operator);

        result.IsSuccess.Should().BeTrue();
        tenant.Members.Should().ContainSingle(m =>
            m.UserId == userId && m.Role == TenantRole.Operator);
    }

    [Fact]
    public void AddMember_WithUndefinedRole_ShouldReturnInvalidRoleError()
    {
        var tenant = Tenant.Create("Clinic").Value;

        var result = tenant.AddMember(Guid.NewGuid(), (TenantRole)999);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Tenant.Errors.InvalidRole);
    }

    [Fact]
    public void AddMember_WithOwnerRole_ShouldSucceed()
    {
        var tenant = Tenant.Create("Clinic").Value;
        var userId = Guid.NewGuid();

        var result = tenant.AddMember(userId, TenantRole.Owner);

        result.IsSuccess.Should().BeTrue();
        tenant.Members.Should().ContainSingle(m => m.UserId == userId && m.Role == TenantRole.Owner);
    }

    [Fact]
    public void AddMember_WithValidData_ShouldReturnSuccessAndAddMember()
    {
        var tenant = Tenant.Create("Clínica").Value;
        var userId = Guid.NewGuid();

        var result = tenant.AddMember(userId, TenantRole.Operator);

        result.IsSuccess.Should().BeTrue();
        tenant.Members.Should().ContainSingle(m => m.UserId == userId && m.Role == TenantRole.Operator);
    }
}

public class TenantMemberUpdateRoleTests
{
    [Fact]
    public void UpdateRole_WithUndefinedRole_ShouldReturnInvalidRoleError()
    {
        var tenant = Tenant.Create("Clinic").Value;
        var userId = Guid.NewGuid();
        tenant.AddMember(userId, TenantRole.Operator);

        var result = tenant.Members[0].UpdateRole((TenantRole)999);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Tenant.Errors.InvalidRole);
    }

    [Fact]
    public void UpdateRole_WithValidRole_ShouldUpdateRole()
    {
        var tenant = Tenant.Create("Clinic").Value;
        var userId = Guid.NewGuid();
        tenant.AddMember(userId, TenantRole.Operator);

        var result = tenant.Members[0].UpdateRole(TenantRole.Doctor);

        result.IsSuccess.Should().BeTrue();
        tenant.Members[0].Role.Should().Be(TenantRole.Doctor);
    }
}

public class TenantActivateDeactivateTests
{
    [Fact]
    public void Deactivate_SetIsActiveToFalse()
    {
        var tenant = Tenant.Create("Clinic").Value;

        tenant.Deactivate();

        tenant.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_WhenInactive_SetIsActiveToTrue()
    {
        var tenant = Tenant.Create("Clinic").Value;
        tenant.Deactivate();

        tenant.Activate();

        tenant.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_RemainsTrue()
    {
        var tenant = Tenant.Create("Clinic").Value;

        tenant.Activate();

        tenant.IsActive.Should().BeTrue();
    }
}

public class TenantRemoveMemberTests
{
    [Fact]
    public void RemoveMember_WhenUserNotMember_ShouldReturnFailure()
    {
        var tenant = Tenant.Create("Clínica").Value;

        var result = tenant.RemoveMember(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Tenant.Errors.MemberNotFound);
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void RemoveMember_WhenUserIsMember_ShouldReturnSuccessAndRemove()
    {
        var tenant = Tenant.Create("Clínica").Value;
        var userId = Guid.NewGuid();
        tenant.AddMember(userId, TenantRole.Operator);

        var result = tenant.RemoveMember(userId);

        result.IsSuccess.Should().BeTrue();
        tenant.Members.Should().BeEmpty();
    }

    [Fact]
    public void RemoveMember_WhenUserIsOwner_ShouldReturnOwnerCannotBeRemoved()
    {
        var tenant = Tenant.Create("Clínica").Value;
        var userId = Guid.NewGuid();
        tenant.AddMember(userId, TenantRole.Owner);

        var result = tenant.RemoveMember(userId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Tenant.Errors.OwnerCannotBeRemoved);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }
}
