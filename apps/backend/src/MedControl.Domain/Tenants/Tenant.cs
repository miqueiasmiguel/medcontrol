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

    public static class Errors
    {
        public static readonly Error NameRequired = Error.Validation("Tenant.NameRequired", "Tenant name is required.");
        public static readonly Error MemberAlreadyExists = Error.Conflict("Tenant.MemberAlreadyExists", "User is already a member of this tenant.");
        public static readonly Error MemberNotFound = Error.NotFound("Tenant.MemberNotFound", "User is not a member of this tenant.");
        public static readonly Error RoleRequired = Error.Validation("Tenant.RoleRequired", "Role is required.");
    }

    public static Result<Tenant> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Tenant>(Errors.NameRequired);
        }

        var slug = GenerateSlug(name);
        var tenant = new Tenant
        {
            Name = name.Trim(),
            Slug = slug,
        };

        tenant.Raise(new TenantCreatedEvent(tenant.Id, tenant.Name, DateTimeOffset.UtcNow));
        return tenant;
    }

    public Result Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(Errors.NameRequired);
        }

        Name = name.Trim();
        return Result.Success();
    }

    public Result AddMember(Guid userId, string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return Result.Failure(Errors.RoleRequired);
        }

        if (_members.Any(m => m.UserId == userId))
        {
            return Result.Failure(Errors.MemberAlreadyExists);
        }

        _members.Add(TenantMember.Create(Id, userId, role));
        return Result.Success();
    }

    public Result RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);

        if (member is null)
        {
            return Result.Failure(Errors.MemberNotFound);
        }

        _members.Remove(member);
        return Result.Success();
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
