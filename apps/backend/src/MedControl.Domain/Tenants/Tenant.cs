using MedControl.Domain.Common;
using MedControl.Domain.Tenants.Events;

namespace MedControl.Domain.Tenants;

public sealed class Tenant : BaseAuditableEntity, IAggregateRoot
{
    private readonly List<TenantMember> _members = [];

    private Tenant() { } // EF Core

    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    public IReadOnlyList<TenantMember> Members => _members.AsReadOnly();

    public static Tenant Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var slug = GenerateSlug(name);
        var tenant = new Tenant
        {
            Name = name.Trim(),
            Slug = slug,
        };

        tenant.Raise(new TenantCreatedEvent(tenant.Id, tenant.Name, DateTimeOffset.UtcNow));
        return tenant;
    }

    public void Update(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
    }

    public void AddMember(Guid userId, string role)
    {
        if (_members.Any(m => m.UserId == userId))
        {
            throw new InvalidOperationException($"User {userId} is already a member of this tenant.");
        }

        _members.Add(TenantMember.Create(Id, userId, role));
    }

    public void RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new InvalidOperationException($"User {userId} is not a member of this tenant.");

        _members.Remove(member);
    }

    public void Deactivate() => IsActive = false;

    private static string GenerateSlug(string name) =>
        name.Trim()
#pragma warning disable CA1308 // slugs must be lowercase by convention
            .ToLowerInvariant()
#pragma warning restore CA1308
            .Replace(" ", "-", StringComparison.Ordinal)
            .Replace("_", "-", StringComparison.Ordinal);
}
