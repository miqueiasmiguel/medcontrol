using FluentValidation;

namespace MedControl.Application.HealthPlans.Commands.UpdateHealthPlan;

public sealed class UpdateHealthPlanCommandValidator : AbstractValidator<UpdateHealthPlanCommand>
{
    public UpdateHealthPlanCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.TissCode).NotEmpty().MaximumLength(20);
    }
}
