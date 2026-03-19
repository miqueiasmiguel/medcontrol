using FluentValidation;

namespace MedControl.Application.Payments.Commands.UpdatePaymentItemStatus;

public sealed class UpdatePaymentItemStatusCommandValidator : AbstractValidator<UpdatePaymentItemStatusCommand>
{
    public UpdatePaymentItemStatusCommandValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}
