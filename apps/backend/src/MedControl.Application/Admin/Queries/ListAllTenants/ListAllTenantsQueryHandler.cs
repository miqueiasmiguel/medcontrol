using MedControl.Application.Admin.DTOs;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;

namespace MedControl.Application.Admin.Queries.ListAllTenants;

public sealed class ListAllTenantsQueryHandler(
    ICurrentUserService currentUser,
    ITenantRepository tenantRepository)
    : IRequestHandler<ListAllTenantsQuery, Result<IReadOnlyList<AdminTenantDto>>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Admin.Unauthorized", "Global admin role required.");

    public async Task<Result<IReadOnlyList<AdminTenantDto>>> Handle(
        ListAllTenantsQuery request,
        CancellationToken ct)
    {
        if (!currentUser.HasGlobalRole("admin"))
        {
            return Result.Failure<IReadOnlyList<AdminTenantDto>>(Unauthorized);
        }

        var tenants = await tenantRepository.ListAllAsync(ct);

        IReadOnlyList<AdminTenantDto> dtos = tenants
            .Select(t => new AdminTenantDto(t.Id, t.Name, t.Slug, t.IsActive, t.CreatedAt, t.Members.Count))
            .ToList();

        return Result.Success(dtos);
    }
}
