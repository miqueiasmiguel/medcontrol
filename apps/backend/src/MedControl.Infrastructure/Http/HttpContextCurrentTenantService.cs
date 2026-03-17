using MedControl.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace MedControl.Infrastructure.Http;

internal sealed class HttpContextCurrentTenantService(IHttpContextAccessor httpContextAccessor)
    : ICurrentTenantService
{
    public Guid? TenantId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User?.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public bool HasTenant => TenantId.HasValue;
}
