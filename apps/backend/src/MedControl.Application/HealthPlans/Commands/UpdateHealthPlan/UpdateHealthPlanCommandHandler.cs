using MedControl.Application.Common.Interfaces;
using MedControl.Application.HealthPlans.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;
using MedControl.Domain.HealthPlans;

namespace MedControl.Application.HealthPlans.Commands.UpdateHealthPlan;

public sealed class UpdateHealthPlanCommandHandler(
    IHealthPlanRepository healthPlanRepository,
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenant)
    : IRequestHandler<UpdateHealthPlanCommand, Result<HealthPlanDto>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("HealthPlan.Unauthorized", "A tenant context is required.");

    private static readonly Error NotFound =
        Error.NotFound("HealthPlan.NotFound", "Health plan not found.");

    private static readonly Error TissCodeAlreadyExists =
        Error.Conflict("HealthPlan.TissCodeAlreadyExists", "A health plan with this TISS code already exists in this tenant.");

    public async Task<Result<HealthPlanDto>> Handle(UpdateHealthPlanCommand request, CancellationToken ct)
    {
        if (currentTenant.TenantId is null)
        {
            return Result.Failure<HealthPlanDto>(Unauthorized);
        }

        var tenantId = currentTenant.TenantId.Value;

        var healthPlan = await healthPlanRepository.GetByIdAsync(request.Id, ct);
        if (healthPlan is null)
        {
            return Result.Failure<HealthPlanDto>(NotFound);
        }

        var tissCodeChanged = healthPlan.TissCode != request.TissCode.Trim();
        if (tissCodeChanged)
        {
            var alreadyExists = await healthPlanRepository.ExistsByTissCodeAsync(tenantId, request.TissCode, ct);
            if (alreadyExists)
            {
                return Result.Failure<HealthPlanDto>(TissCodeAlreadyExists);
            }
        }

        var updateResult = healthPlan.Update(request.Name, request.TissCode);
        if (updateResult.IsFailure)
        {
            return Result.Failure<HealthPlanDto>(updateResult.Error);
        }

        await healthPlanRepository.UpdateAsync(healthPlan, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new HealthPlanDto(
            healthPlan.Id,
            healthPlan.TenantId,
            healthPlan.Name,
            healthPlan.TissCode));
    }
}
