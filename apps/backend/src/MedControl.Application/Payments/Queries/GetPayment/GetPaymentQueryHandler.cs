using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using MedControl.Application.Payments.Commands.CreatePayment;
using MedControl.Application.Payments.DTOs;
using MedControl.Domain.Common;
using MedControl.Domain.Payments;

namespace MedControl.Application.Payments.Queries.GetPayment;

public sealed class GetPaymentQueryHandler(
    IPaymentRepository paymentRepository,
    ICurrentUserService currentUser)
    : IRequestHandler<GetPaymentQuery, Result<PaymentDto>>
{
    private static readonly Error NotFound =
        Error.NotFound("Payment.NotFound", "Payment not found.");

    private static readonly Error Unauthorized =
        Error.Unauthorized("Payment.Unauthorized", "Access to this payment is not allowed.");

    public async Task<Result<PaymentDto>> Handle(GetPaymentQuery request, CancellationToken ct)
    {
        var payment = await paymentRepository.GetByIdAsync(request.Id, ct);
        if (payment is null)
        {
            return Result.Failure<PaymentDto>(NotFound);
        }

        if (currentUser.Roles.Contains("doctor", StringComparer.OrdinalIgnoreCase) &&
            currentUser.UserId.HasValue &&
            payment.DoctorId != currentUser.UserId.Value)
        {
            return Result.Failure<PaymentDto>(Unauthorized);
        }

        return Result.Success(CreatePaymentCommandHandler.MapToDto(payment));
    }
}
