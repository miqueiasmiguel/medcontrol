namespace MedControl.Application.Payments.DTOs;

public sealed record PaymentItemDto(
    Guid Id,
    Guid ProcedureId,
    decimal Value,
    string Status,
    string? Notes);
