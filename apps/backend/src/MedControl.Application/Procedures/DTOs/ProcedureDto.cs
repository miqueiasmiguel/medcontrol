namespace MedControl.Application.Procedures.DTOs;

public sealed record ProcedureDto(
    Guid Id,
    Guid TenantId,
    string Code,
    string Description,
    decimal Value);
