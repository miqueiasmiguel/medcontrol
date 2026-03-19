using FluentAssertions;
using MedControl.Domain.Common;
using MedControl.Domain.Procedures;

namespace MedControl.Domain.Tests.Procedures;

public class ProcedureCreateTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public void Create_ComDadosValidos_DeveRetornarSucesso()
    {
        var tenantId = Guid.NewGuid();

        var result = Procedure.Create(tenantId, "10101012", "Consulta médica", 150.00m, Today);

        result.IsSuccess.Should().BeTrue();
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.Code.Should().Be("10101012");
        result.Value.Description.Should().Be("Consulta médica");
        result.Value.Value.Should().Be(150.00m);
        result.Value.EffectiveFrom.Should().Be(Today);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ComCodeInvalido_DeveRetornarFalha(string? code)
    {
        var result = Procedure.Create(Guid.NewGuid(), code!, "Consulta médica", 150.00m, Today);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Procedure.Errors.CodeRequired);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ComDescriptionInvalida_DeveRetornarFalha(string? description)
    {
        var result = Procedure.Create(Guid.NewGuid(), "10101012", description!, 150.00m, Today);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Procedure.Errors.DescriptionRequired);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Create_ComValueInvalido_DeveRetornarFalha(double value)
    {
        var result = Procedure.Create(Guid.NewGuid(), "10101012", "Consulta médica", (decimal)value, Today);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Procedure.Errors.ValueInvalid);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }
}

public class ProcedureUpdateTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public void Update_ComDadosValidos_DeveAtualizarCampos()
    {
        var procedure = Procedure.Create(Guid.NewGuid(), "10101012", "Consulta médica", 150.00m, Today).Value;

        var result = procedure.Update("20202025", "Consulta especializada", 300.00m);

        result.IsSuccess.Should().BeTrue();
        procedure.Code.Should().Be("20202025");
        procedure.Description.Should().Be("Consulta especializada");
        procedure.Value.Should().Be(300.00m);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_ComCodeInvalido_DeveRetornarFalha(string? code)
    {
        var procedure = Procedure.Create(Guid.NewGuid(), "10101012", "Consulta médica", 150.00m, Today).Value;

        var result = procedure.Update(code!, "Consulta médica", 150.00m);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Procedure.Errors.CodeRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_ComDescriptionInvalida_DeveRetornarFalha(string? description)
    {
        var procedure = Procedure.Create(Guid.NewGuid(), "10101012", "Consulta médica", 150.00m, Today).Value;

        var result = procedure.Update("10101012", description!, 150.00m);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Procedure.Errors.DescriptionRequired);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Update_ComValueInvalido_DeveRetornarFalha(double value)
    {
        var procedure = Procedure.Create(Guid.NewGuid(), "10101012", "Consulta médica", 150.00m, Today).Value;

        var result = procedure.Update("10101012", "Consulta médica", (decimal)value);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Procedure.Errors.ValueInvalid);
    }

    [Fact]
    public void Update_ComEffectiveTo_DeveAtualizarEffectiveTo()
    {
        var procedure = Procedure.Create(Guid.NewGuid(), "10101012", "Consulta médica", 150.00m, Today).Value;
        var effectiveTo = Today.AddDays(365);

        var result = procedure.Update("10101012", "Consulta médica", 150.00m, effectiveTo);

        result.IsSuccess.Should().BeTrue();
        procedure.EffectiveTo.Should().Be(effectiveTo);
    }
}
