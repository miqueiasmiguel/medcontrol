using FluentValidation;

namespace MedControl.Application.Payments.Commands.UpdatePayment;

public sealed class UpdatePaymentCommandValidator : AbstractValidator<UpdatePaymentCommand>
{
    public UpdatePaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
        RuleFor(x => x.AppointmentNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BeneficiaryCard).NotEmpty().MaximumLength(50);
        RuleFor(x => x.BeneficiaryName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.ExecutionLocation).NotEmpty().MaximumLength(256);
        RuleFor(x => x.PaymentLocation).NotEmpty().MaximumLength(256);
    }
}
