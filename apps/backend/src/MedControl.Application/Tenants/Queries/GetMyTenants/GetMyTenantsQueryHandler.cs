using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using MedControl.Application.Tenants.DTOs;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;

namespace MedControl.Application.Tenants.Queries.GetMyTenants;

public sealed class GetMyTenantsQueryHandler(
    ICurrentUserService currentUser,
    ITenantRepository tenantRepository)
    : IRequestHandler<GetMyTenantsQuery, Result<IReadOnlyList<TenantDto>>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Tenant.Unauthorized", "User is not authenticated.");

    public async Task<Result<IReadOnlyList<TenantDto>>> Handle(GetMyTenantsQuery request, CancellationToken ct)
    {
        if (currentUser.UserId is null)
        {
            return Result.Failure<IReadOnlyList<TenantDto>>(Unauthorized);
        }

        var userId = currentUser.UserId.Value;
        var tenants = await tenantRepository.ListByUserAsync(userId, ct);

        var dtos = tenants
            .Where(t => t.IsActive)
            .Select(t =>
            {
                var member = t.Members.FirstOrDefault(m => m.UserId == userId);
                var role = member?.Role.ToString().ToLowerInvariant() ?? string.Empty;
                return new TenantDto(t.Id, t.Name, t.Slug, role);
            })
            .ToList();

        return Result.Success<IReadOnlyList<TenantDto>>(dtos);
    }
}
