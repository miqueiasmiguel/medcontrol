using MedControl.Application.Common.Interfaces;
using MedControl.Application.HealthPlans.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;
using MedControl.Domain.HealthPlans;

namespace MedControl.Application.HealthPlans.Commands.CreateHealthPlan;

public sealed class CreateHealthPlanCommandHandler(
    IHealthPlanRepository healthPlanRepository,
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenant)
    : IRequestHandler<CreateHealthPlanCommand, Result<HealthPlanDto>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("HealthPlan.Unauthorized", "A tenant context is required.");

    private static readonly Error TissCodeAlreadyExists =
        Error.Conflict("HealthPlan.TissCodeAlreadyExists", "A health plan with this TISS code already exists in this tenant.");

    public async Task<Result<HealthPlanDto>> Handle(CreateHealthPlanCommand request, CancellationToken ct)
    {
        if (currentTenant.TenantId is null)
        {
            return Result.Failure<HealthPlanDto>(Unauthorized);
        }

        var tenantId = currentTenant.TenantId.Value;

        var alreadyExists = await healthPlanRepository.ExistsByTissCodeAsync(tenantId, request.TissCode, ct);
        if (alreadyExists)
        {
            return Result.Failure<HealthPlanDto>(TissCodeAlreadyExists);
        }

        var healthPlanResult = HealthPlan.Create(tenantId, request.Name, request.TissCode);
        if (healthPlanResult.IsFailure)
        {
            return Result.Failure<HealthPlanDto>(healthPlanResult.Error);
        }

        var healthPlan = healthPlanResult.Value;
        await healthPlanRepository.AddAsync(healthPlan, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new HealthPlanDto(
            healthPlan.Id,
            healthPlan.TenantId,
            healthPlan.Name,
            healthPlan.TissCode));
    }
}
