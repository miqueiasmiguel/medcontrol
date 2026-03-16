using MedControl.Domain.Common;

namespace MedControl.Domain.Tenants;

public sealed class TenantMember : BaseEntity
{
    private TenantMember() { } // EF Core

    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public string Role { get; private set; } = default!;
    public DateTimeOffset JoinedAt { get; private set; }

    internal static TenantMember Create(Guid tenantId, Guid userId, string role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);

        return new TenantMember
        {
            TenantId = tenantId,
            UserId = userId,
            Role = role,
            JoinedAt = DateTimeOffset.UtcNow,
        };
    }

    public void UpdateRole(string role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);
        Role = role;
    }
}
