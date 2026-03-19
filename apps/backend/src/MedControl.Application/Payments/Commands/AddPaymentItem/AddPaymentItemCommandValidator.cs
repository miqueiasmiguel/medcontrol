using FluentValidation;

namespace MedControl.Application.Payments.Commands.AddPaymentItem;

public sealed class AddPaymentItemCommandValidator : AbstractValidator<AddPaymentItemCommand>
{
    public AddPaymentItemCommandValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
        RuleFor(x => x.ProcedureId).NotEmpty();
        RuleFor(x => x.Value).GreaterThan(0);
    }
}
