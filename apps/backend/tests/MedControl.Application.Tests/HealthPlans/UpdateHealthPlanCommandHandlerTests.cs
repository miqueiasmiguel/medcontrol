using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.HealthPlans.Commands.UpdateHealthPlan;
using MedControl.Domain.Common;
using MedControl.Domain.HealthPlans;
using NSubstitute;

namespace MedControl.Application.Tests.HealthPlans;

public sealed class UpdateHealthPlanCommandHandlerTests
{
    private readonly IHealthPlanRepository _healthPlanRepository = Substitute.For<IHealthPlanRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentTenantService _currentTenant = Substitute.For<ICurrentTenantService>();
    private readonly UpdateHealthPlanCommandHandler _sut;

    public UpdateHealthPlanCommandHandlerTests()
    {
        _sut = new UpdateHealthPlanCommandHandler(_healthPlanRepository, _unitOfWork, _currentTenant);
    }

    [Fact]
    public async Task Handle_ComDadosValidos_DeveAtualizarEPersistir()
    {
        var tenantId = Guid.NewGuid();
        var healthPlan = HealthPlan.Create(tenantId, "Unimed", "11111119").Value;
        _currentTenant.TenantId.Returns(tenantId);
        _healthPlanRepository.GetByIdAsync(healthPlan.Id, Arg.Any<CancellationToken>()).Returns(healthPlan);
        _healthPlanRepository.ExistsByTissCodeAsync(tenantId, "22222224", Arg.Any<CancellationToken>()).Returns(false);

        var command = new UpdateHealthPlanCommand(healthPlan.Id, "Amil", "22222224");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Amil");
        result.Value.TissCode.Should().Be("22222224");
        await _healthPlanRepository.Received(1).UpdateAsync(healthPlan, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SemTenant_DeveRetornarUnauthorized()
    {
        _currentTenant.TenantId.Returns((Guid?)null);

        var command = new UpdateHealthPlanCommand(Guid.NewGuid(), "Unimed", "11111119");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_ConvenioNaoEncontrado_DeveRetornarNotFound()
    {
        var tenantId = Guid.NewGuid();
        var healthPlanId = Guid.NewGuid();
        _currentTenant.TenantId.Returns(tenantId);
        _healthPlanRepository.GetByIdAsync(healthPlanId, Arg.Any<CancellationToken>()).Returns((HealthPlan?)null);

        var command = new UpdateHealthPlanCommand(healthPlanId, "Unimed", "11111119");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        await _healthPlanRepository.DidNotReceive().UpdateAsync(Arg.Any<HealthPlan>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TissCodeDuplicadoEmOutroConvenio_DeveRetornarConflict()
    {
        var tenantId = Guid.NewGuid();
        var healthPlan = HealthPlan.Create(tenantId, "Unimed", "11111119").Value;
        _currentTenant.TenantId.Returns(tenantId);
        _healthPlanRepository.GetByIdAsync(healthPlan.Id, Arg.Any<CancellationToken>()).Returns(healthPlan);
        _healthPlanRepository.ExistsByTissCodeAsync(tenantId, "99999999", Arg.Any<CancellationToken>()).Returns(true);

        var command = new UpdateHealthPlanCommand(healthPlan.Id, "Unimed", "99999999");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        await _healthPlanRepository.DidNotReceive().UpdateAsync(Arg.Any<HealthPlan>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MesmoTissCodeDoProprioConvenio_DevePermitirAtualizar()
    {
        var tenantId = Guid.NewGuid();
        var healthPlan = HealthPlan.Create(tenantId, "Unimed", "11111119").Value;
        _currentTenant.TenantId.Returns(tenantId);
        _healthPlanRepository.GetByIdAsync(healthPlan.Id, Arg.Any<CancellationToken>()).Returns(healthPlan);

        var command = new UpdateHealthPlanCommand(healthPlan.Id, "Unimed SP", "11111119");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Validator_ComIdVazio_DeveRetornarErroDeValidacao()
    {
        var validator = new UpdateHealthPlanCommandValidator();
        var validation = await validator.ValidateAsync(
            new UpdateHealthPlanCommand(Guid.Empty, "Unimed", "11111119"));
        validation.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_ComNomeVazio_DeveRetornarErroDeValidacao()
    {
        var validator = new UpdateHealthPlanCommandValidator();
        var validation = await validator.ValidateAsync(
            new UpdateHealthPlanCommand(Guid.NewGuid(), string.Empty, "11111119"));
        validation.IsValid.Should().BeFalse();
    }
}
