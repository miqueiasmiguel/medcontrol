using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Procedures.Commands.CreateProcedure;
using MedControl.Domain.Common;
using MedControl.Domain.Procedures;
using NSubstitute;

namespace MedControl.Application.Tests.Procedures;

public sealed class CreateProcedureCommandHandlerTests
{
    private readonly IProcedureRepository _procedureRepository = Substitute.For<IProcedureRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentTenantService _currentTenant = Substitute.For<ICurrentTenantService>();
    private readonly CreateProcedureCommandHandler _sut;

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    public CreateProcedureCommandHandlerTests()
    {
        _sut = new CreateProcedureCommandHandler(_procedureRepository, _unitOfWork, _currentTenant);
    }

    [Fact]
    public async Task Handle_ComDadosValidos_DeveAdicionarAoRepositorio()
    {
        var tenantId = Guid.NewGuid();
        _currentTenant.TenantId.Returns(tenantId);
        _procedureRepository.ExistsByCodeAndEffectiveFromAsync(
            tenantId, "10101012", Today, Arg.Any<CancellationToken>()).Returns(false);

        var command = new CreateProcedureCommand("10101012", "Consulta médica", 150.00m, Today);
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Code.Should().Be("10101012");
        result.Value.Description.Should().Be("Consulta médica");
        result.Value.Value.Should().Be(150.00m);
        result.Value.EffectiveFrom.Should().Be(Today);
        await _procedureRepository.Received(1).AddAsync(
            Arg.Is<Procedure>(p =>
                p.Code == "10101012" &&
                p.Description == "Consulta médica" &&
                p.Value == 150.00m &&
                p.TenantId == tenantId &&
                p.EffectiveFrom == Today),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ComCodeEEffectiveFromDuplicados_DeveRetornarConflict()
    {
        var tenantId = Guid.NewGuid();
        _currentTenant.TenantId.Returns(tenantId);
        _procedureRepository.ExistsByCodeAndEffectiveFromAsync(
            tenantId, "10101012", Today, Arg.Any<CancellationToken>()).Returns(true);

        var command = new CreateProcedureCommand("10101012", "Consulta médica", 150.00m, Today);
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        await _procedureRepository.DidNotReceive().AddAsync(Arg.Any<Procedure>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SemTenant_DeveRetornarUnauthorized()
    {
        _currentTenant.TenantId.Returns((Guid?)null);

        var command = new CreateProcedureCommand("10101012", "Consulta médica", 150.00m, Today);
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Validator_ComCodeVazio_DeveRetornarErroDeValidacao()
    {
        var validator = new CreateProcedureCommandValidator();
        var validation = await validator.ValidateAsync(new CreateProcedureCommand(string.Empty, "Consulta médica", 150.00m, Today));
        validation.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_ComDescriptionVazia_DeveRetornarErroDeValidacao()
    {
        var validator = new CreateProcedureCommandValidator();
        var validation = await validator.ValidateAsync(new CreateProcedureCommand("10101012", string.Empty, 150.00m, Today));
        validation.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_ComValueZero_DeveRetornarErroDeValidacao()
    {
        var validator = new CreateProcedureCommandValidator();
        var validation = await validator.ValidateAsync(new CreateProcedureCommand("10101012", "Consulta médica", 0m, Today));
        validation.IsValid.Should().BeFalse();
    }
}
