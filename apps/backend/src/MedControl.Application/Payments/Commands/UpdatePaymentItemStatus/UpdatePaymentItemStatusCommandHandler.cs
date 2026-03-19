using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using MedControl.Application.Payments.Commands.CreatePayment;
using MedControl.Application.Payments.DTOs;
using MedControl.Domain.Common;
using MedControl.Domain.Payments;

namespace MedControl.Application.Payments.Commands.UpdatePaymentItemStatus;

public sealed class UpdatePaymentItemStatusCommandHandler(
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenant)
    : IRequestHandler<UpdatePaymentItemStatusCommand, Result<PaymentDto>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Payment.Unauthorized", "A tenant context is required.");

    private static readonly Error NotFound =
        Error.NotFound("Payment.NotFound", "Payment not found.");

    public async Task<Result<PaymentDto>> Handle(UpdatePaymentItemStatusCommand request, CancellationToken ct)
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

        var itemResult = payment.GetItem(request.ItemId);
        if (itemResult.IsFailure)
        {
            return Result.Failure<PaymentDto>(itemResult.Error);
        }

        itemResult.Value.UpdateStatus(request.Status, request.Notes);
        await paymentRepository.UpdateAsync(payment, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(CreatePaymentCommandHandler.MapToDto(payment));
    }
}
