using MedControl.Domain.Common;

namespace MedControl.Domain.Procedures;

public sealed class ProcedureImport : BaseAuditableEntity, IAggregateRoot, IHasTenant
{
    private ProcedureImport() { } // EF Core

    public Guid TenantId { get; private set; }
    public ProcedureSource Source { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public int TotalRows { get; private set; }
    public int ImportedRows { get; private set; }
    public int SkippedRows { get; private set; }
    public string? ErrorSummary { get; private set; }

    public static class Errors
    {
        public static readonly Error ManualSourceNotAllowed =
            Error.Validation("ProcedureImport.ManualSourceNotAllowed", "Manual source is not allowed for imports.");
    }

    public static Result<ProcedureImport> Create(
        Guid tenantId,
        ProcedureSource source,
        DateOnly effectiveFrom,
        int totalRows,
        int importedRows,
        int skippedRows,
        string? errorSummary)
    {
        if (source == ProcedureSource.Manual)
        {
            return Result.Failure<ProcedureImport>(Errors.ManualSourceNotAllowed);
        }

        return new ProcedureImport
        {
            TenantId = tenantId,
            Source = source,
            EffectiveFrom = effectiveFrom,
            TotalRows = totalRows,
            ImportedRows = importedRows,
            SkippedRows = skippedRows,
            ErrorSummary = errorSummary is null ? null : errorSummary[..Math.Min(errorSummary.Length, 2000)],
        };
    }
}
