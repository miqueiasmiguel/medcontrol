using MedControl.Application.Mediator;
using MedControl.Application.Payments.DTOs;
using MedControl.Domain.Common;
using MedControl.Domain.Payments;

namespace MedControl.Application.Payments.Commands.UpdatePaymentItemStatus;

public record UpdatePaymentItemStatusCommand(
    Guid PaymentId,
    Guid ItemId,
    PaymentStatus Status,
    string? Notes) : ICommand<Result<PaymentDto>>;
