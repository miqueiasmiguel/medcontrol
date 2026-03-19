using MedControl.Application.Mediator;
using MedControl.Application.Payments.DTOs;
using MedControl.Domain.Common;

namespace MedControl.Application.Payments.Commands.AddPaymentItem;

public record AddPaymentItemCommand(
    Guid PaymentId,
    Guid ProcedureId,
    decimal Value) : ICommand<Result<PaymentDto>>;
