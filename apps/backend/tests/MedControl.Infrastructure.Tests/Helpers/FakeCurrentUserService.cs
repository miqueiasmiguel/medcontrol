using MedControl.Application.Common.Interfaces;

namespace MedControl.Infrastructure.Tests.Helpers;

internal sealed class FakeCurrentUserService : ICurrentUserService
{
    public Guid? UserId => null;
    public Guid? TenantId => null;
    public string? Email => null;
    public IReadOnlyList<string> Roles => [];
    public IReadOnlyList<string> GlobalRoles => [];
    public bool IsAuthenticated => false;
    public bool HasGlobalRole(string role) => false;
}
