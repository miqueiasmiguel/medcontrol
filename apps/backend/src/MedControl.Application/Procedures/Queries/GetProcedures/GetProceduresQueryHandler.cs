using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using MedControl.Application.Procedures.DTOs;
using MedControl.Domain.Common;
using MedControl.Domain.Procedures;

namespace MedControl.Application.Procedures.Queries.GetProcedures;

public sealed class GetProceduresQueryHandler(
    IProcedureRepository procedureRepository,
    ICurrentTenantService currentTenant)
    : IRequestHandler<GetProceduresQuery, Result<IReadOnlyList<ProcedureDto>>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Procedure.Unauthorized", "A tenant context is required.");

    public async Task<Result<IReadOnlyList<ProcedureDto>>> Handle(GetProceduresQuery request, CancellationToken ct)
    {
        if (currentTenant.TenantId is null)
        {
            return Result.Failure<IReadOnlyList<ProcedureDto>>(Unauthorized);
        }

        var procedures = await procedureRepository.ListAsync(ct);

        var dtos = procedures
            .Select(p => new ProcedureDto(p.Id, p.TenantId, p.Code, p.Description, p.Value))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<ProcedureDto>>(dtos);
    }
}
