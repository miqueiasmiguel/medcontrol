using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using MedControl.Application.Payments.Commands.CreatePayment;
using MedControl.Application.Payments.DTOs;
using MedControl.Domain.Common;
using MedControl.Domain.Payments;

namespace MedControl.Application.Payments.Commands.UpdatePayment;

public sealed class UpdatePaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenant)
    : IRequestHandler<UpdatePaymentCommand, Result<PaymentDto>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Payment.Unauthorized", "A tenant context is required.");

    private static readonly Error NotFound =
        Error.NotFound("Payment.NotFound", "Payment not found.");

    public async Task<Result<PaymentDto>> Handle(UpdatePaymentCommand request, CancellationToken ct)
    {
        if (currentTenant.TenantId is null)
        {
            return Result.Failure<PaymentDto>(Unauthorized);
        }

        var payment = await paymentRepository.GetByIdAsync(request.PaymentId, ct);
        if (payment is null)
        {
            return Result.Failure<PaymentDto>(NotFound);
        }

        var updateResult = payment.Update(
            request.ExecutionDate,
            request.AppointmentNumber,
            request.AuthorizationCode,
            request.BeneficiaryCard,
            request.BeneficiaryName,
            request.ExecutionLocation,
            request.PaymentLocation,
            request.Notes);

        if (updateResult.IsFailure)
        {
            return Result.Failure<PaymentDto>(updateResult.Error);
        }

        await paymentRepository.UpdateAsync(payment, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(CreatePaymentCommandHandler.MapToDto(payment));
    }
}
