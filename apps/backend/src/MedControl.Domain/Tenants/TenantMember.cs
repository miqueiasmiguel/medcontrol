using MedControl.Domain.Common;

namespace MedControl.Domain.Tenants;

public sealed class TenantMember : BaseEntity
{
    private TenantMember() { } // EF Core

    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public TenantRole Role { get; private set; }
    public DateTimeOffset JoinedAt { get; private set; }

    internal static TenantMember Create(Guid tenantId, Guid userId, TenantRole role)
    {
        return new TenantMember
        {
            TenantId = tenantId,
            UserId = userId,
            Role = role,
            JoinedAt = DateTimeOffset.UtcNow,
        };
    }

    public Result UpdateRole(TenantRole role)
    {
        if (!Enum.IsDefined(role))
        {
            return Result.Failure(Tenant.Errors.InvalidRole);
        }

        Role = role;
        return Result.Success();
    }
}
