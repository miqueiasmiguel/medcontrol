using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using MedControl.Application.Procedures.DTOs;
using MedControl.Domain.Common;
using MedControl.Domain.Procedures;

namespace MedControl.Application.Procedures.Commands.CreateProcedure;

public sealed class CreateProcedureCommandHandler(
    IProcedureRepository procedureRepository,
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenant)
    : IRequestHandler<CreateProcedureCommand, Result<ProcedureDto>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Procedure.Unauthorized", "A tenant context is required.");

    private static readonly Error CodeAlreadyExists =
        Error.Conflict("Procedure.CodeAlreadyExists", "A procedure with this code and effective date already exists in this tenant.");

    public async Task<Result<ProcedureDto>> Handle(CreateProcedureCommand request, CancellationToken ct)
    {
        if (currentTenant.TenantId is null)
        {
            return Result.Failure<ProcedureDto>(Unauthorized);
        }

        var tenantId = currentTenant.TenantId.Value;

        var alreadyExists = await procedureRepository.ExistsByCodeAndEffectiveFromAsync(
            tenantId, request.Code, request.EffectiveFrom, ct);
        if (alreadyExists)
        {
            return Result.Failure<ProcedureDto>(CodeAlreadyExists);
        }

        var procedureResult = Procedure.Create(
            tenantId, request.Code, request.Description, request.Value,
            request.EffectiveFrom, request.EffectiveTo);
        if (procedureResult.IsFailure)
        {
            return Result.Failure<ProcedureDto>(procedureResult.Error);
        }

        var procedure = procedureResult.Value;
        await procedureRepository.AddAsync(procedure, ct);
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
