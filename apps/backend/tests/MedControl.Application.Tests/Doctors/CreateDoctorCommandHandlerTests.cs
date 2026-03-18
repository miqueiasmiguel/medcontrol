using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Doctors.Commands.CreateDoctor;
using MedControl.Domain.Common;
using MedControl.Domain.Doctors;
using NSubstitute;

namespace MedControl.Application.Tests.Doctors;

public sealed class CreateDoctorCommandHandlerTests
{
    private readonly IDoctorRepository _doctorRepository = Substitute.For<IDoctorRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentTenantService _currentTenant = Substitute.For<ICurrentTenantService>();
    private readonly CreateDoctorCommandHandler _sut;

    public CreateDoctorCommandHandlerTests()
    {
        _sut = new CreateDoctorCommandHandler(_doctorRepository, _unitOfWork, _currentTenant);
    }

    [Fact]
    public async Task Handle_ComDadosValidos_DeveAdicionarAoRepositorio()
    {
        var tenantId = Guid.NewGuid();
        _currentTenant.TenantId.Returns(tenantId);
        _doctorRepository.ExistsByCrmAsync(tenantId, "123456", "SP", Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateDoctorCommand("Dr. João Silva", "123456", "SP", "Cardiologia");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Dr. João Silva");
        result.Value.Crm.Should().Be("123456");
        result.Value.CouncilState.Should().Be("SP");
        result.Value.Specialty.Should().Be("Cardiologia");
        await _doctorRepository.Received(1).AddAsync(
            Arg.Is<DoctorProfile>(d =>
                d.Name == "Dr. João Silva" &&
                d.Crm == "123456" &&
                d.TenantId == tenantId),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ComCrmDuplicado_DeveRetornarConflict()
    {
        var tenantId = Guid.NewGuid();
        _currentTenant.TenantId.Returns(tenantId);
        _doctorRepository.ExistsByCrmAsync(tenantId, "123456", "SP", Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new CreateDoctorCommand("Dr. João Silva", "123456", "SP", "Cardiologia");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        await _doctorRepository.DidNotReceive().AddAsync(Arg.Any<DoctorProfile>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SemTenant_DeveRetornarUnauthorized()
    {
        _currentTenant.TenantId.Returns((Guid?)null);

        var command = new CreateDoctorCommand("Dr. João", "123456", "SP", "Cardiologia");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Validator_ComNomeVazio_DeveRetornarErroDeValidacao()
    {
        var validator = new CreateDoctorCommandValidator();
        var validation = await validator.ValidateAsync(new CreateDoctorCommand(string.Empty, "123456", "SP", "Cardiologia"));
        validation.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_ComCrmVazio_DeveRetornarErroDeValidacao()
    {
        var validator = new CreateDoctorCommandValidator();
        var validation = await validator.ValidateAsync(new CreateDoctorCommand("Dr. João", string.Empty, "SP", "Cardiologia"));
        validation.IsValid.Should().BeFalse();
    }
}
