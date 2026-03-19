using MedControl.Application.Mediator;
using MedControl.Application.Payments.Commands.CreatePayment;
using MedControl.Application.Payments.DTOs;
using MedControl.Domain.Common;
using MedControl.Domain.Payments;

namespace MedControl.Application.Payments.Queries.ListPayments;

public sealed class ListPaymentsQueryHandler(IPaymentRepository paymentRepository)
    : IRequestHandler<ListPaymentsQuery, Result<IReadOnlyList<PaymentDto>>>
{
    public async Task<Result<IReadOnlyList<PaymentDto>>> Handle(ListPaymentsQuery request, CancellationToken ct)
    {
        var payments = await paymentRepository.ListAsync(ct);
        IReadOnlyList<PaymentDto> dtos = payments.Select(CreatePaymentCommandHandler.MapToDto).ToList();
        return Result.Success(dtos);
    }
}
