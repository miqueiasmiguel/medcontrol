using FluentValidation;

namespace MedControl.Application.Procedures.Commands.UpdateProcedure;

public sealed class UpdateProcedureCommandValidator : AbstractValidator<UpdateProcedureCommand>
{
    public UpdateProcedureCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(512);
        RuleFor(x => x.Value).GreaterThan(0);
    }
}
