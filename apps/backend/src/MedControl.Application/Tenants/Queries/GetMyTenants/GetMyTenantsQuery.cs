using MedControl.Application.Mediator;
using MedControl.Application.Tenants.DTOs;
using MedControl.Domain.Common;

namespace MedControl.Application.Tenants.Queries.GetMyTenants;

public record GetMyTenantsQuery : IQuery<Result<IReadOnlyList<TenantDto>>>;
