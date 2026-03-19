using FluentValidation;

namespace MedControl.Application.Procedures.Commands.CreateProcedure;

public sealed class CreateProcedureCommandValidator : AbstractValidator<CreateProcedureCommand>
{
    public CreateProcedureCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(512);
        RuleFor(x => x.Value).GreaterThan(0);
        RuleFor(x => x.EffectiveFrom).NotEmpty();
    }
}
