using MedControl.Application.Mediator;
using MedControl.Application.Procedures.DTOs;
using MedControl.Domain.Common;

namespace MedControl.Application.Procedures.Commands.UpdateProcedure;

public record UpdateProcedureCommand(
    Guid Id,
    string Code,
    string Description,
    decimal Value,
    DateOnly? EffectiveTo = null) : ICommand<Result<ProcedureDto>>;
