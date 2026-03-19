using FluentAssertions;
using MedControl.Domain.Common;
using MedControl.Domain.Procedures;

namespace MedControl.Domain.Tests.Procedures;

public class ProcedureVigenciaTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public void Create_ComEffectiveFromValido_DeveSetarPropriedades()
    {
        var tenantId = Guid.NewGuid();
        var effectiveFrom = new DateOnly(2025, 1, 1);
        var effectiveTo = new DateOnly(2025, 12, 31);

        var result = Procedure.Create(tenantId, "10101012", "Consulta médica", 150.00m, effectiveFrom, effectiveTo);

        result.IsSuccess.Should().BeTrue();
        result.Value.EffectiveFrom.Should().Be(effectiveFrom);
        result.Value.EffectiveTo.Should().Be(effectiveTo);
        result.Value.Source.Should().Be(ProcedureSource.Manual);
    }

    [Fact]
    public void Create_ComEffectiveToNull_DeveRetornarSucesso()
    {
        var tenantId = Guid.NewGuid();
        var effectiveFrom = new DateOnly(2025, 1, 1);

        var result = Procedure.Create(tenantId, "10101012", "Consulta médica", 150.00m, effectiveFrom);

        result.IsSuccess.Should().BeTrue();
        result.Value.EffectiveTo.Should().BeNull();
    }

    [Fact]
    public void Create_ComSourceTuss_DeveSetarSource()
    {
        var tenantId = Guid.NewGuid();
        var effectiveFrom = new DateOnly(2025, 1, 1);

        var result = Procedure.Create(tenantId, "10101012", "Consulta médica", 150.00m, effectiveFrom, null, ProcedureSource.Tuss);

        result.IsSuccess.Should().BeTrue();
        result.Value.Source.Should().Be(ProcedureSource.Tuss);
    }

    [Fact]
    public void Create_ComEffectiveToMenorQueEffectiveFrom_DeveRetornarFalha()
    {
        var tenantId = Guid.NewGuid();
        var effectiveFrom = new DateOnly(2025, 6, 1);
        var effectiveTo = new DateOnly(2025, 5, 31);

        var result = Procedure.Create(tenantId, "10101012", "Consulta médica", 150.00m, effectiveFrom, effectiveTo);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Procedure.Errors.EffectiveDateRangeInvalid);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Create_ComEffectiveToIgualEffectiveFrom_DeveRetornarFalha()
    {
        var tenantId = Guid.NewGuid();
        var effectiveFrom = new DateOnly(2025, 6, 1);

        var result = Procedure.Create(tenantId, "10101012", "Consulta médica", 150.00m, effectiveFrom, effectiveFrom);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Procedure.Errors.EffectiveDateRangeInvalid);
    }

    [Fact]
    public void CloseVigencia_DeveSetarEffectiveTo()
    {
        var procedure = Procedure.Create(
            Guid.NewGuid(), "10101012", "Consulta médica", 150.00m,
            new DateOnly(2025, 1, 1)).Value;

        procedure.CloseVigencia(new DateOnly(2025, 12, 31));

        procedure.EffectiveTo.Should().Be(new DateOnly(2025, 12, 31));
    }
}
