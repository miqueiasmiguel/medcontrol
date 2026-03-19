using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using MedControl.Application.Procedures.DTOs;
using MedControl.Domain.Common;
using MedControl.Domain.Procedures;

namespace MedControl.Application.Procedures.Commands.UpdateProcedure;

public sealed class UpdateProcedureCommandHandler(
    IProcedureRepository procedureRepository,
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenant)
    : IRequestHandler<UpdateProcedureCommand, Result<ProcedureDto>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Procedure.Unauthorized", "A tenant context is required.");

    private static readonly Error NotFound =
        Error.NotFound("Procedure.NotFound", "Procedure not found.");

    private static readonly Error CodeAlreadyExists =
        Error.Conflict("Procedure.CodeAlreadyExists", "A procedure with this code already exists in this tenant.");

    public async Task<Result<ProcedureDto>> Handle(UpdateProcedureCommand request, CancellationToken ct)
    {
        if (currentTenant.TenantId is null)
        {
            return Result.Failure<ProcedureDto>(Unauthorized);
        }

        var tenantId = currentTenant.TenantId.Value;

        var procedure = await procedureRepository.GetByIdAsync(request.Id, ct);
        if (procedure is null)
        {
            return Result.Failure<ProcedureDto>(NotFound);
        }

        var codeChanged = procedure.Code != request.Code.Trim();
        if (codeChanged)
        {
            var alreadyExists = await procedureRepository.ExistsByCodeAsync(tenantId, request.Code, ct);
            if (alreadyExists)
            {
                return Result.Failure<ProcedureDto>(CodeAlreadyExists);
            }
        }

        var updateResult = procedure.Update(request.Code, request.Description, request.Value, request.EffectiveTo);
        if (updateResult.IsFailure)
        {
            return Result.Failure<ProcedureDto>(updateResult.Error);
        }

        await procedureRepository.UpdateAsync(procedure, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new ProcedureDto(
            procedure.Id,
            procedure.TenantId,
            procedure.Code,
            procedure.Description,
            procedure.Value,
            procedure.EffectiveFrom,
            procedure.EffectiveTo,
            procedure.Source.ToString()));
    }
}
