using MedControl.Application.Mediator;
using MedControl.Application.Payments.DTOs;
using MedControl.Domain.Common;
using MedControl.Domain.Payments;

namespace MedControl.Application.Payments.Queries.ListPayments;

public record ListPaymentsQuery(PaymentFilters Filters) : IQuery<Result<IReadOnlyList<PaymentDto>>>;
