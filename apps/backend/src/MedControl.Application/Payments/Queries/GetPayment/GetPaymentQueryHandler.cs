using MedControl.Application.Mediator;
using MedControl.Application.Payments.Commands.CreatePayment;
using MedControl.Application.Payments.DTOs;
using MedControl.Domain.Common;
using MedControl.Domain.Payments;

namespace MedControl.Application.Payments.Queries.GetPayment;

public sealed class GetPaymentQueryHandler(IPaymentRepository paymentRepository)
    : IRequestHandler<GetPaymentQuery, Result<PaymentDto>>
{
    private static readonly Error NotFound =
        Error.NotFound("Payment.NotFound", "Payment not found.");

    public async Task<Result<PaymentDto>> Handle(GetPaymentQuery request, CancellationToken ct)
    {
        var payment = await paymentRepository.GetByIdAsync(request.Id, ct);
        if (payment is null)
        {
            return Result.Failure<PaymentDto>(NotFound);
        }

        return Result.Success(CreatePaymentCommandHandler.MapToDto(payment));
    }
}
