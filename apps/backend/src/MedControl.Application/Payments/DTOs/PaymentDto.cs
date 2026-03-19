namespace MedControl.Application.Payments.DTOs;

public sealed record PaymentDto(
    Guid Id,
    Guid TenantId,
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
    string Status,
    IReadOnlyList<PaymentItemDto> Items);
