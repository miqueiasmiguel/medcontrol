using FluentAssertions;
using MedControl.Domain.Common;
using MedControl.Domain.HealthPlans;

namespace MedControl.Domain.Tests.HealthPlans;

public class HealthPlanCreateTests
{
    [Fact]
    public void Create_ComDadosValidos_DeveRetornarSucesso()
    {
        var tenantId = Guid.NewGuid();

        var result = HealthPlan.Create(tenantId, "Unimed", "11111119");

        result.IsSuccess.Should().BeTrue();
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.Name.Should().Be("Unimed");
        result.Value.TissCode.Should().Be("11111119");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ComNomeInvalido_DeveRetornarFalha(string? name)
    {
        var result = HealthPlan.Create(Guid.NewGuid(), name!, "11111119");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(HealthPlan.Errors.NameRequired);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ComTissCodeInvalido_DeveRetornarFalha(string? tissCode)
    {
        var result = HealthPlan.Create(Guid.NewGuid(), "Unimed", tissCode!);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(HealthPlan.Errors.TissCodeRequired);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }
}

public class HealthPlanUpdateTests
{
    [Fact]
    public void Update_ComDadosValidos_DeveAtualizarCampos()
    {
        var healthPlan = HealthPlan.Create(Guid.NewGuid(), "Unimed", "11111119").Value;

        var result = healthPlan.Update("Amil", "22222224");

        result.IsSuccess.Should().BeTrue();
        healthPlan.Name.Should().Be("Amil");
        healthPlan.TissCode.Should().Be("22222224");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_ComNomeInvalido_DeveRetornarFalha(string? name)
    {
        var healthPlan = HealthPlan.Create(Guid.NewGuid(), "Unimed", "11111119").Value;

        var result = healthPlan.Update(name!, "11111119");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(HealthPlan.Errors.NameRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_ComTissCodeInvalido_DeveRetornarFalha(string? tissCode)
    {
        var healthPlan = HealthPlan.Create(Guid.NewGuid(), "Unimed", "11111119").Value;

        var result = healthPlan.Update("Unimed", tissCode!);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(HealthPlan.Errors.TissCodeRequired);
    }
}
