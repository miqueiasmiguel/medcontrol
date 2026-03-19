using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using MedControl.Application.Procedures.DTOs;
using MedControl.Domain.Common;
using MedControl.Domain.Procedures;

namespace MedControl.Application.Procedures.Queries.GetProcedureImports;

public sealed class GetProcedureImportsQueryHandler(
    IProcedureImportRepository importRepository,
    ICurrentTenantService currentTenant)
    : IRequestHandler<GetProcedureImportsQuery, Result<IReadOnlyList<ProcedureImportDto>>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Procedure.Unauthorized", "A tenant context is required.");

    public async Task<Result<IReadOnlyList<ProcedureImportDto>>> Handle(
        GetProcedureImportsQuery request, CancellationToken ct)
    {
        if (currentTenant.TenantId is null)
        {
            return Result.Failure<IReadOnlyList<ProcedureImportDto>>(Unauthorized);
        }

        var imports = await importRepository.ListAsync(ct);

        var dtos = imports
            .Select(i => new ProcedureImportDto(
                i.Id,
                i.Source.ToString(),
                i.EffectiveFrom,
                i.TotalRows,
                i.ImportedRows,
                i.SkippedRows,
                i.ErrorSummary,
                i.CreatedAt))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<ProcedureImportDto>>(dtos);
    }
}
