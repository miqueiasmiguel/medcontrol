using FluentValidation;

namespace MedControl.Application.HealthPlans.Commands.CreateHealthPlan;

public sealed class CreateHealthPlanCommandValidator : AbstractValidator<CreateHealthPlanCommand>
{
    public CreateHealthPlanCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.TissCode).NotEmpty().MaximumLength(20);
    }
}
