using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using MedControl.Application.Payments.Commands.CreatePayment;
using MedControl.Application.Payments.DTOs;
using MedControl.Domain.Common;
using MedControl.Domain.Payments;

namespace MedControl.Application.Payments.Queries.ListPayments;

public sealed class ListPaymentsQueryHandler(
    IPaymentRepository paymentRepository,
    ICurrentUserService currentUser)
    : IRequestHandler<ListPaymentsQuery, Result<IReadOnlyList<PaymentDto>>>
{
    public async Task<Result<IReadOnlyList<PaymentDto>>> Handle(ListPaymentsQuery request, CancellationToken ct)
    {
        var filters = EnforceDoctorFilter(request.Filters);
        var payments = await paymentRepository.ListAsync(filters, ct);
        IReadOnlyList<PaymentDto> dtos = payments.Select(CreatePaymentCommandHandler.MapToDto).ToList();
        return Result.Success(dtos);
    }

    private PaymentFilters EnforceDoctorFilter(PaymentFilters filters)
    {
        if (currentUser.Roles.Contains("doctor", StringComparer.OrdinalIgnoreCase) &&
            currentUser.UserId.HasValue)
        {
            return filters with { DoctorId = currentUser.UserId.Value };
        }

        return filters;
    }
}
