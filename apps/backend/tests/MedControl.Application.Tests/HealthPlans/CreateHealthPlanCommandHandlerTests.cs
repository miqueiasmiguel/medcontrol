using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.HealthPlans.Commands.CreateHealthPlan;
using MedControl.Domain.Common;
using MedControl.Domain.HealthPlans;
using NSubstitute;

namespace MedControl.Application.Tests.HealthPlans;

public sealed class CreateHealthPlanCommandHandlerTests
{
    private readonly IHealthPlanRepository _healthPlanRepository = Substitute.For<IHealthPlanRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentTenantService _currentTenant = Substitute.For<ICurrentTenantService>();
    private readonly CreateHealthPlanCommandHandler _sut;

    public CreateHealthPlanCommandHandlerTests()
    {
        _sut = new CreateHealthPlanCommandHandler(_healthPlanRepository, _unitOfWork, _currentTenant);
    }

    [Fact]
    public async Task Handle_ComDadosValidos_DeveAdicionarAoRepositorio()
    {
        var tenantId = Guid.NewGuid();
        _currentTenant.TenantId.Returns(tenantId);
        _healthPlanRepository.ExistsByTissCodeAsync(tenantId, "11111119", Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateHealthPlanCommand("Unimed", "11111119");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Unimed");
        result.Value.TissCode.Should().Be("11111119");
        await _healthPlanRepository.Received(1).AddAsync(
            Arg.Is<HealthPlan>(hp =>
                hp.Name == "Unimed" &&
                hp.TissCode == "11111119" &&
                hp.TenantId == tenantId),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ComTissCodeDuplicado_DeveRetornarConflict()
    {
        var tenantId = Guid.NewGuid();
        _currentTenant.TenantId.Returns(tenantId);
        _healthPlanRepository.ExistsByTissCodeAsync(tenantId, "11111119", Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new CreateHealthPlanCommand("Unimed", "11111119");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        await _healthPlanRepository.DidNotReceive().AddAsync(Arg.Any<HealthPlan>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SemTenant_DeveRetornarUnauthorized()
    {
        _currentTenant.TenantId.Returns((Guid?)null);

        var command = new CreateHealthPlanCommand("Unimed", "11111119");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Validator_ComNomeVazio_DeveRetornarErroDeValidacao()
    {
        var validator = new CreateHealthPlanCommandValidator();
        var validation = await validator.ValidateAsync(new CreateHealthPlanCommand(string.Empty, "11111119"));
        validation.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_ComTissCodeVazio_DeveRetornarErroDeValidacao()
    {
        var validator = new CreateHealthPlanCommandValidator();
        var validation = await validator.ValidateAsync(new CreateHealthPlanCommand("Unimed", string.Empty));
        validation.IsValid.Should().BeFalse();
    }
}
