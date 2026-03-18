using MedControl.Application.HealthPlans.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Application.HealthPlans.Commands.UpdateHealthPlan;

public record UpdateHealthPlanCommand(
    Guid Id,
    string Name,
    string TissCode) : ICommand<Result<HealthPlanDto>>;
