using MedControl.Application.Mediator;
using MedControl.Application.Procedures.DTOs;
using MedControl.Domain.Common;
using MedControl.Domain.Procedures;

namespace MedControl.Application.Procedures.Commands.ImportProcedures;

public record ImportProceduresCommand(
    Stream CsvStream,
    ProcedureSource Source,
    DateOnly EffectiveFrom) : ICommand<Result<ProcedureImportDto>>;
