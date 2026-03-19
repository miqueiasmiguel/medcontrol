using FluentAssertions;
using MedControl.Domain.Common;
using MedControl.Domain.Procedures;

namespace MedControl.Domain.Tests.Procedures;

public class ProcedureImportTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly DateOnly EffectiveFrom = new(2025, 1, 1);

    [Fact]
    public void Create_ComSourceTuss_DeveRetornarSucesso()
    {
        var result = ProcedureImport.Create(TenantId, ProcedureSource.Tuss, EffectiveFrom, 100, 95, 5, null);

        result.IsSuccess.Should().BeTrue();
        result.Value.TenantId.Should().Be(TenantId);
        result.Value.Source.Should().Be(ProcedureSource.Tuss);
        result.Value.EffectiveFrom.Should().Be(EffectiveFrom);
        result.Value.TotalRows.Should().Be(100);
        result.Value.ImportedRows.Should().Be(95);
        result.Value.SkippedRows.Should().Be(5);
        result.Value.ErrorSummary.Should().BeNull();
    }

    [Fact]
    public void Create_ComSourceCbhpm_DeveRetornarSucesso()
    {
        var result = ProcedureImport.Create(TenantId, ProcedureSource.Cbhpm, EffectiveFrom, 50, 50, 0, null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Source.Should().Be(ProcedureSource.Cbhpm);
    }

    [Fact]
    public void Create_ComSourceManual_DeveRetornarFalha()
    {
        var result = ProcedureImport.Create(TenantId, ProcedureSource.Manual, EffectiveFrom, 10, 10, 0, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProcedureImport.Errors.ManualSourceNotAllowed);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Create_ComErrorSummaryLongo_DeveTruncarEm2000Chars()
    {
        var errorSummary = new string('x', 3000);

        var result = ProcedureImport.Create(TenantId, ProcedureSource.Tuss, EffectiveFrom, 100, 90, 10, errorSummary);

        result.IsSuccess.Should().BeTrue();
        result.Value.ErrorSummary.Should().HaveLength(2000);
    }

    [Fact]
    public void Create_ComContagensCorretas_DeveSetarContagens()
    {
        var result = ProcedureImport.Create(TenantId, ProcedureSource.Tuss, EffectiveFrom, 200, 180, 20, "linha 5: valor inválido");

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalRows.Should().Be(200);
        result.Value.ImportedRows.Should().Be(180);
        result.Value.SkippedRows.Should().Be(20);
        result.Value.ErrorSummary.Should().Be("linha 5: valor inválido");
    }
}
