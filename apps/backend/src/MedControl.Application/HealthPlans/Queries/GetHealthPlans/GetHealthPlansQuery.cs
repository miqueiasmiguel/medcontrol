using MedControl.Application.HealthPlans.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Application.HealthPlans.Queries.GetHealthPlans;

public record GetHealthPlansQuery : IQuery<Result<IReadOnlyList<HealthPlanDto>>>;
