using MedControl.Application.Mediator;
using MedControl.Application.Payments.DTOs;
using MedControl.Domain.Common;

namespace MedControl.Application.Payments.Queries.GetPayment;

public record GetPaymentQuery(Guid Id) : IQuery<Result<PaymentDto>>;
