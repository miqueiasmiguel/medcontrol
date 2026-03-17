using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MedControl.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace MedControl.Infrastructure.Http;

internal sealed class HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value = User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public Guid? TenantId
    {
        get
        {
            var value = User?.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Email => User?.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

    public IReadOnlyList<string> Roles =>
        User?.FindAll("roles").Select(c => c.Value).ToList() ?? [];

    public IReadOnlyList<string> GlobalRoles =>
        User?.FindAll("global_roles").Select(c => c.Value).ToList() ?? [];

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool HasGlobalRole(string role) =>
        GlobalRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
}
