using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using MedControl.Application.Procedures.DTOs;
using MedControl.Application.Procedures.Parsers;
using MedControl.Domain.Common;
using MedControl.Domain.Procedures;

namespace MedControl.Application.Procedures.Commands.ImportProcedures;

public sealed class ImportProceduresCommandHandler(
    IEnumerable<IProcedureFileParser> parsers,
    IProcedureRepository procedureRepository,
    IProcedureImportRepository importRepository,
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenant)
    : IRequestHandler<ImportProceduresCommand, Result<ProcedureImportDto>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Procedure.Unauthorized", "A tenant context is required.");

    private static readonly Error ParserNotFound =
        Error.Validation("Procedure.ParserNotFound", "No parser available for the specified source.");

    public async Task<Result<ProcedureImportDto>> Handle(ImportProceduresCommand request, CancellationToken ct)
    {
        if (currentTenant.TenantId is null)
        {
            return Result.Failure<ProcedureImportDto>(Unauthorized);
        }

        var tenantId = currentTenant.TenantId.Value;

        var parser = parsers.FirstOrDefault(p => p.Source == request.Source);
        if (parser is null)
        {
            return Result.Failure<ProcedureImportDto>(ParserNotFound);
        }

        var parseResult = parser.Parse(request.CsvStream);

        var importedCount = 0;
        var skippedCount = parseResult.SkippedCount;

        var proceduresToAdd = new List<Procedure>();

        foreach (var row in parseResult.Rows)
        {
            var exists = await procedureRepository.ExistsByCodeAndEffectiveFromAsync(
                tenantId, row.Code, request.EffectiveFrom, ct);

            if (exists)
            {
                skippedCount++;
                continue;
            }

            var procedureResult = Procedure.Create(
                tenantId,
                row.Code,
                row.Description,
                row.Value,
                request.EffectiveFrom,
                row.EffectiveTo,
                request.Source);

            if (procedureResult.IsFailure)
            {
                skippedCount++;
                continue;
            }

            proceduresToAdd.Add(procedureResult.Value);
            importedCount++;
        }

        if (proceduresToAdd.Count > 0)
        {
            await procedureRepository.AddRangeAsync(proceduresToAdd, ct);
        }

        var totalRows = parseResult.Rows.Count + parseResult.SkippedCount;
        var importResult = ProcedureImport.Create(
            tenantId,
            request.Source,
            request.EffectiveFrom,
            totalRows,
            importedCount,
            skippedCount,
            parseResult.ErrorSummary);

        if (importResult.IsFailure)
        {
            return Result.Failure<ProcedureImportDto>(importResult.Error);
        }

        await importRepository.AddAsync(importResult.Value, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var import = importResult.Value;
        return Result.Success(new ProcedureImportDto(
            import.Id,
            import.Source.ToString(),
            import.EffectiveFrom,
            import.TotalRows,
            import.ImportedRows,
            import.SkippedRows,
            import.ErrorSummary,
            import.CreatedAt));
    }
}
