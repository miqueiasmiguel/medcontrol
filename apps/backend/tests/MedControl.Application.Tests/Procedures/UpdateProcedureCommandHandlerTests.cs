using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Procedures.Commands.UpdateProcedure;
using MedControl.Domain.Common;
using MedControl.Domain.Procedures;
using NSubstitute;

namespace MedControl.Application.Tests.Procedures;

public sealed class UpdateProcedureCommandHandlerTests
{
    private readonly IProcedureRepository _procedureRepository = Substitute.For<IProcedureRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentTenantService _currentTenant = Substitute.For<ICurrentTenantService>();
    private readonly UpdateProcedureCommandHandler _sut;

    public UpdateProcedureCommandHandlerTests()
    {
        _sut = new UpdateProcedureCommandHandler(_procedureRepository, _unitOfWork, _currentTenant);
    }

    [Fact]
    public async Task Handle_ComDadosValidos_DeveAtualizarEPersistir()
    {
        var tenantId = Guid.NewGuid();
        var procedure = Procedure.Create(tenantId, "10101012", "Consulta médica", 150.00m).Value;
        _currentTenant.TenantId.Returns(tenantId);
        _procedureRepository.GetByIdAsync(procedure.Id, Arg.Any<CancellationToken>()).Returns(procedure);
        _procedureRepository.ExistsByCodeAsync(tenantId, "20202025", Arg.Any<CancellationToken>()).Returns(false);

        var command = new UpdateProcedureCommand(procedure.Id, "20202025", "Consulta especializada", 300.00m);
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Code.Should().Be("20202025");
        result.Value.Description.Should().Be("Consulta especializada");
        result.Value.Value.Should().Be(300.00m);
        await _procedureRepository.Received(1).UpdateAsync(procedure, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SemTenant_DeveRetornarUnauthorized()
    {
        _currentTenant.TenantId.Returns((Guid?)null);

        var command = new UpdateProcedureCommand(Guid.NewGuid(), "10101012", "Consulta médica", 150.00m);
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_ProcedimentoNaoEncontrado_DeveRetornarNotFound()
    {
        var tenantId = Guid.NewGuid();
        var procedureId = Guid.NewGuid();
        _currentTenant.TenantId.Returns(tenantId);
        _procedureRepository.GetByIdAsync(procedureId, Arg.Any<CancellationToken>()).Returns((Procedure?)null);

        var command = new UpdateProcedureCommand(procedureId, "10101012", "Consulta médica", 150.00m);
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        await _procedureRepository.DidNotReceive().UpdateAsync(Arg.Any<Procedure>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CodeDuplicadoEmOutroProcedimento_DeveRetornarConflict()
    {
        var tenantId = Guid.NewGuid();
        var procedure = Procedure.Create(tenantId, "10101012", "Consulta médica", 150.00m).Value;
        _currentTenant.TenantId.Returns(tenantId);
        _procedureRepository.GetByIdAsync(procedure.Id, Arg.Any<CancellationToken>()).Returns(procedure);
        _procedureRepository.ExistsByCodeAsync(tenantId, "99999999", Arg.Any<CancellationToken>()).Returns(true);

        var command = new UpdateProcedureCommand(procedure.Id, "99999999", "Consulta médica", 150.00m);
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        await _procedureRepository.DidNotReceive().UpdateAsync(Arg.Any<Procedure>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MesmoCodeDoProprioProcedimento_DevePermitirAtualizar()
    {
        var tenantId = Guid.NewGuid();
        var procedure = Procedure.Create(tenantId, "10101012", "Consulta médica", 150.00m).Value;
        _currentTenant.TenantId.Returns(tenantId);
        _procedureRepository.GetByIdAsync(procedure.Id, Arg.Any<CancellationToken>()).Returns(procedure);

        var command = new UpdateProcedureCommand(procedure.Id, "10101012", "Consulta médica atualizada", 200.00m);
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Validator_ComIdVazio_DeveRetornarErroDeValidacao()
    {
        var validator = new UpdateProcedureCommandValidator();
        var validation = await validator.ValidateAsync(
            new UpdateProcedureCommand(Guid.Empty, "10101012", "Consulta médica", 150.00m));
        validation.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_ComCodeVazio_DeveRetornarErroDeValidacao()
    {
        var validator = new UpdateProcedureCommandValidator();
        var validation = await validator.ValidateAsync(
            new UpdateProcedureCommand(Guid.NewGuid(), string.Empty, "Consulta médica", 150.00m));
        validation.IsValid.Should().BeFalse();
    }
}
