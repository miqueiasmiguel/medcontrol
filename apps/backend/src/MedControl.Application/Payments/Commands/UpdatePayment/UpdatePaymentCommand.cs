using MedControl.Application.Mediator;
using MedControl.Application.Payments.DTOs;
using MedControl.Domain.Common;

namespace MedControl.Application.Payments.Commands.UpdatePayment;

public record UpdatePaymentCommand(
    Guid PaymentId,
    DateOnly ExecutionDate,
    string AppointmentNumber,
    string? AuthorizationCode,
    string BeneficiaryCard,
    string BeneficiaryName,
    string ExecutionLocation,
    string PaymentLocation,
    string? Notes) : ICommand<Result<PaymentDto>>;
