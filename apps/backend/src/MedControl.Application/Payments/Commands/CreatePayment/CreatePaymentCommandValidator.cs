using FluentValidation;

namespace MedControl.Application.Payments.Commands.CreatePayment;

public sealed class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.HealthPlanId).NotEmpty();
        RuleFor(x => x.ExecutionDate).NotEmpty();
        RuleFor(x => x.AppointmentNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BeneficiaryCard).NotEmpty().MaximumLength(50);
        RuleFor(x => x.BeneficiaryName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.ExecutionLocation).NotEmpty().MaximumLength(256);
        RuleFor(x => x.PaymentLocation).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProcedureId).NotEmpty();
            item.RuleFor(i => i.Value).GreaterThan(0);
        });
    }
}
