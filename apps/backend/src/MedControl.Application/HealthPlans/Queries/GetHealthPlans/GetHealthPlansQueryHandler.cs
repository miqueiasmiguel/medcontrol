using MedControl.Application.Common.Interfaces;
using MedControl.Application.HealthPlans.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;
using MedControl.Domain.HealthPlans;

namespace MedControl.Application.HealthPlans.Queries.GetHealthPlans;

public sealed class GetHealthPlansQueryHandler(
    IHealthPlanRepository healthPlanRepository,
    ICurrentTenantService currentTenant)
    : IRequestHandler<GetHealthPlansQuery, Result<IReadOnlyList<HealthPlanDto>>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("HealthPlan.Unauthorized", "A tenant context is required.");

    public async Task<Result<IReadOnlyList<HealthPlanDto>>> Handle(GetHealthPlansQuery request, CancellationToken ct)
    {
        if (currentTenant.TenantId is null)
        {
            return Result.Failure<IReadOnlyList<HealthPlanDto>>(Unauthorized);
        }

        var healthPlans = await healthPlanRepository.ListAsync(ct);

        var dtos = healthPlans
            .Select(hp => new HealthPlanDto(hp.Id, hp.TenantId, hp.Name, hp.TissCode))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<HealthPlanDto>>(dtos);
    }
}
