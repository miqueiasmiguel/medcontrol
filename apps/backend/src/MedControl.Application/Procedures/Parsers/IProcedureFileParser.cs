using MedControl.Domain.Procedures;

namespace MedControl.Application.Procedures.Parsers;

public interface IProcedureFileParser
{
    ProcedureSource Source { get; }
    ParseResult Parse(Stream csvStream);
}

public sealed record ParsedProcedureRow(string Code, string Description, decimal Value, DateOnly? EffectiveTo);

public sealed record ParseResult(IReadOnlyList<ParsedProcedureRow> Rows, int SkippedCount, string? ErrorSummary);
