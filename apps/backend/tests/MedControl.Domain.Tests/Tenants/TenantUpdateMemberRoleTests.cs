using FluentAssertions;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;

namespace MedControl.Domain.Tests.Tenants;

public class TenantUpdateMemberRoleTests
{
    [Fact]
    public void UpdateMemberRole_WhenUserNotMember_ShouldReturnMemberNotFound()
    {
        var tenant = Tenant.Create("Clinic").Value;

        var result = tenant.UpdateMemberRole(Guid.NewGuid(), Guid.NewGuid(), TenantRole.Admin);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Tenant.Errors.MemberNotFound);
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void UpdateMemberRole_WhenUpdatingOwnRole_ShouldReturnCannotUpdateOwnRole()
    {
        var tenant = Tenant.Create("Clinic").Value;
        var userId = Guid.NewGuid();
        tenant.AddMember(userId, TenantRole.Operator);

        var result = tenant.UpdateMemberRole(userId, userId, TenantRole.Admin);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Tenant.Errors.CannotUpdateOwnRole);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void UpdateMemberRole_WithUndefinedRole_ShouldReturnInvalidRole()
    {
        var tenant = Tenant.Create("Clinic").Value;
        var userId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        tenant.AddMember(userId, TenantRole.Operator);

        var result = tenant.UpdateMemberRole(userId, currentUserId, (TenantRole)999);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Tenant.Errors.InvalidRole);
    }

    [Fact]
    public void UpdateMemberRole_WithValidData_ShouldUpdateRole()
    {
        var tenant = Tenant.Create("Clinic").Value;
        var userId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        tenant.AddMember(userId, TenantRole.Operator);

        var result = tenant.UpdateMemberRole(userId, currentUserId, TenantRole.Doctor);

        result.IsSuccess.Should().BeTrue();
        tenant.Members.Single(m => m.UserId == userId).Role.Should().Be(TenantRole.Doctor);
    }
}
