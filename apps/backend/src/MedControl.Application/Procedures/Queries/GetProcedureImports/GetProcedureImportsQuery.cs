using MedControl.Application.Mediator;
using MedControl.Application.Procedures.DTOs;
using MedControl.Domain.Common;

namespace MedControl.Application.Procedures.Queries.GetProcedureImports;

public record GetProcedureImportsQuery : IQuery<Result<IReadOnlyList<ProcedureImportDto>>>;
