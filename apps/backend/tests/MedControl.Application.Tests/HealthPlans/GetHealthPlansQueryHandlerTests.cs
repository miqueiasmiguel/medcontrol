using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.HealthPlans.Queries.GetHealthPlans;
using MedControl.Domain.Common;
using MedControl.Domain.HealthPlans;
using NSubstitute;

namespace MedControl.Application.Tests.HealthPlans;

public sealed class GetHealthPlansQueryHandlerTests
{
    private readonly IHealthPlanRepository _healthPlanRepository = Substitute.For<IHealthPlanRepository>();
    private readonly ICurrentTenantService _currentTenant = Substitute.For<ICurrentTenantService>();
    private readonly GetHealthPlansQueryHandler _sut;

    public GetHealthPlansQueryHandlerTests()
    {
        _sut = new GetHealthPlansQueryHandler(_healthPlanRepository, _currentTenant);
    }

    [Fact]
    public async Task Handle_SemTenant_DeveRetornarUnauthorized()
    {
        _currentTenant.TenantId.Returns((Guid?)null);

        var result = await _sut.Handle(new GetHealthPlansQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_ComTenant_DeveRetornarListaMapeada()
    {
        var tenantId = Guid.NewGuid();
        _currentTenant.TenantId.Returns(tenantId);
        var healthPlan1 = HealthPlan.Create(tenantId, "Unimed", "11111119").Value;
        var healthPlan2 = HealthPlan.Create(tenantId, "Amil", "22222224").Value;
        _healthPlanRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<HealthPlan> { healthPlan1, healthPlan2 });

        var result = await _sut.Handle(new GetHealthPlansQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Name.Should().Be("Unimed");
        result.Value[1].Name.Should().Be("Amil");
    }

    [Fact]
    public async Task Handle_ComListaVazia_DeveRetornarListaVazia()
    {
        var tenantId = Guid.NewGuid();
        _currentTenant.TenantId.Returns(tenantId);
        _healthPlanRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<HealthPlan>());

        var result = await _sut.Handle(new GetHealthPlansQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
