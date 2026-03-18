using MedControl.Application.HealthPlans.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Application.HealthPlans.Commands.CreateHealthPlan;

public record CreateHealthPlanCommand(
    string Name,
    string TissCode) : ICommand<Result<HealthPlanDto>>;
