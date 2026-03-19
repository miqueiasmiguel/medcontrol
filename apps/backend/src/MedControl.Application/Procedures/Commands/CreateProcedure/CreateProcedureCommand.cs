using MedControl.Application.Mediator;
using MedControl.Application.Procedures.DTOs;
using MedControl.Domain.Common;

namespace MedControl.Application.Procedures.Commands.CreateProcedure;

public record CreateProcedureCommand(
    string Code,
    string Description,
    decimal Value,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo = null) : ICommand<Result<ProcedureDto>>;
