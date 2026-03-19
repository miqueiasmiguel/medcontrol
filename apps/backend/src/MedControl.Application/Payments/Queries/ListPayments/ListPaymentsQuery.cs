using MedControl.Application.Mediator;
using MedControl.Application.Payments.DTOs;
using MedControl.Domain.Common;

namespace MedControl.Application.Payments.Queries.ListPayments;

public record ListPaymentsQuery : IQuery<Result<IReadOnlyList<PaymentDto>>>;
