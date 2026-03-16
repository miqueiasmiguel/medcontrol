namespace MedControl.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? TenantId { get; }
    string? Email { get; }
    IReadOnlyList<string> Roles { get; }
    IReadOnlyList<string> GlobalRoles { get; }
    bool IsAuthenticated { get; }
    bool HasGlobalRole(string role);
}
