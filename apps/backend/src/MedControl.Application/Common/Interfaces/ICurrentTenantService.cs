namespace MedControl.Application.Common.Interfaces;

public interface ICurrentTenantService
{
    Guid? TenantId { get; }
    bool HasTenant { get; }
}
