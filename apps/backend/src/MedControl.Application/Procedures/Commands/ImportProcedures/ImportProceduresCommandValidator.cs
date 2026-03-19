using FluentValidation;
using MedControl.Domain.Procedures;

namespace MedControl.Application.Procedures.Commands.ImportProcedures;

public sealed class ImportProceduresCommandValidator : AbstractValidator<ImportProceduresCommand>
{
    public ImportProceduresCommandValidator()
    {
        RuleFor(x => x.CsvStream).NotNull();
        RuleFor(x => x.Source)
            .Must(s => s != ProcedureSource.Manual)
            .WithMessage("Source must be Tuss or Cbhpm.");
        RuleFor(x => x.EffectiveFrom).NotEmpty();
    }
}
