using FluentValidation;

namespace MedControl.Application.Payments.Commands.RemovePaymentItem;

public sealed class RemovePaymentItemCommandValidator : AbstractValidator<RemovePaymentItemCommand>
{
    public RemovePaymentItemCommandValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
    }
}
