namespace MedControl.Application.Procedures.DTOs;

public sealed record ProcedureImportDto(
    Guid Id,
    string Source,
    DateOnly EffectiveFrom,
    int TotalRows,
    int ImportedRows,
    int SkippedRows,
    string? ErrorSummary,
    DateTimeOffset CreatedAt);
