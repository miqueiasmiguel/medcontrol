using MedControl.Application.Mediator;
using MedControl.Application.Procedures.DTOs;
using MedControl.Domain.Common;

namespace MedControl.Application.Procedures.Queries.GetProcedures;

public record GetProceduresQuery : IQuery<Result<IReadOnlyList<ProcedureDto>>>;
