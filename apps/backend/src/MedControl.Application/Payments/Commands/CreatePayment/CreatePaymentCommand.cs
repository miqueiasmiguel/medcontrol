using MedControl.Application.Mediator;
using MedControl.Application.Payments.DTOs;
using MedControl.Domain.Common;

namespace MedControl.Application.Payments.Commands.CreatePayment;

public sealed record CreatePaymentItemRequest(
    Guid ProcedureId,
    decimal Value);

public record CreatePaymentCommand(
    Guid DoctorId,
    Guid HealthPlanId,
    DateOnly ExecutionDate,
    string AppointmentNumber,
    string? AuthorizationCode,
    string BeneficiaryCard,
    string BeneficiaryName,
    string ExecutionLocation,
    string PaymentLocation,
    string? Notes,
    IReadOnlyList<CreatePaymentItemRequest> Items) : ICommand<Result<PaymentDto>>;
