namespace MedControl.Domain.Common;

/// <summary>
/// Marker interface for entities that belong to a specific tenant.
/// EF Core global query filters will automatically scope queries by TenantId.
/// </summary>
public interface IHasTenant
{
    Guid TenantId { get; }
}
