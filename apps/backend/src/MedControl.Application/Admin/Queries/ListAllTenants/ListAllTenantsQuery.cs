using MedControl.Application.Admin.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Application.Admin.Queries.ListAllTenants;

public record ListAllTenantsQuery : IQuery<Result<IReadOnlyList<AdminTenantDto>>>;
