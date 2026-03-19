using MedControl.Application.Procedures.Parsers;
using MedControl.Domain.Procedures;

namespace MedControl.Infrastructure.Procedures.Parsers;

public sealed class CbhpmCsvParser : IProcedureFileParser
{
    public ProcedureSource Source => ProcedureSource.Cbhpm;

    public ParseResult Parse(Stream csvStream)
    {
        var rows = new List<ParsedProcedureRow>();
        var skippedCount = 0;
        var errors = new List<string>();

        using var reader = new StreamReader(csvStream, leaveOpen: true);

        // skip header
        var header = reader.ReadLine();
        if (header is null)
        {
            return new ParseResult(rows, 0, null);
        }

        var lineNumber = 1;
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split(';');
            if (parts.Length < 4)
            {
                skippedCount++;
                errors.Add($"linha {lineNumber}: colunas insuficientes");
                continue;
            }

            var code = parts[0].Trim();
            var description = parts[1].Trim();

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(description))
            {
                skippedCount++;
                errors.Add($"linha {lineNumber}: código ou descrição vazio");
                continue;
            }

            if (!decimal.TryParse(parts[2].Trim().Replace(',', '.'),
                    System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var porte))
            {
                skippedCount++;
                errors.Add($"linha {lineNumber}: porte inválido '{parts[2].Trim()}'");
                continue;
            }

            if (!decimal.TryParse(parts[3].Trim().Replace(',', '.'),
                    System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var custoOperacional))
            {
                skippedCount++;
                errors.Add($"linha {lineNumber}: custo operacional inválido '{parts[3].Trim()}'");
                continue;
            }

            var value = porte + custoOperacional;
            if (value <= 0)
            {
                skippedCount++;
                errors.Add($"linha {lineNumber}: valor calculado (porte + custo) deve ser maior que zero");
                continue;
            }

            rows.Add(new ParsedProcedureRow(code, description, value, null));
        }

        string? errorSummary = errors.Count > 0 ? string.Join('\n', errors) : null;
        return new ParseResult(rows, skippedCount, errorSummary);
    }
}
