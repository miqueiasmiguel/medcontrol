using MedControl.Application.Mediator;
using MedControl.Application.Payments.DTOs;
using MedControl.Domain.Common;

namespace MedControl.Application.Payments.Commands.RemovePaymentItem;

public record RemovePaymentItemCommand(
    Guid PaymentId,
    Guid ItemId) : ICommand<Result<PaymentDto>>;
