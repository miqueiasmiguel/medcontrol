using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using MedControl.Application.Payments.DTOs;
using MedControl.Domain.Common;
using MedControl.Domain.Payments;

namespace MedControl.Application.Payments.Commands.CreatePayment;

public sealed class CreatePaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenant)
    : IRequestHandler<CreatePaymentCommand, Result<PaymentDto>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Payment.Unauthorized", "A tenant context is required.");

    public async Task<Result<PaymentDto>> Handle(CreatePaymentCommand request, CancellationToken ct)
    {
        if (currentTenant.TenantId is null)
        {
            return Result.Failure<PaymentDto>(Unauthorized);
        }

        var tenantId = currentTenant.TenantId.Value;

        var paymentResult = Payment.Create(
            tenantId,
            request.DoctorId,
            request.HealthPlanId,
            request.ExecutionDate,
            request.AppointmentNumber,
            request.AuthorizationCode,
            request.BeneficiaryCard,
            request.BeneficiaryName,
            request.ExecutionLocation,
            request.PaymentLocation,
            request.Notes,
            request.Items.Select(i => (i.ProcedureId, i.Value)));

        if (paymentResult.IsFailure)
        {
            return Result.Failure<PaymentDto>(paymentResult.Error);
        }

        var payment = paymentResult.Value;
        await paymentRepository.AddAsync(payment, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(MapToDto(payment));
    }

    internal static PaymentDto MapToDto(Payment payment) => new(
        payment.Id,
        payment.TenantId,
        payment.DoctorId,
        payment.HealthPlanId,
        payment.ExecutionDate,
        payment.AppointmentNumber,
        payment.AuthorizationCode,
        payment.BeneficiaryCard,
        payment.BeneficiaryName,
        payment.ExecutionLocation,
        payment.PaymentLocation,
        payment.Notes,
        payment.Status.ToString(),
        payment.Items.Sum(i => i.Value),
        payment.Items.Select(i => new PaymentItemDto(
            i.Id,
            i.ProcedureId,
            i.Value,
            i.Status.ToString(),
            i.Notes)).ToList());
}
