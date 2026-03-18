using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Doctors.Commands.UpdateDoctor;
using MedControl.Domain.Common;
using MedControl.Domain.Doctors;
using NSubstitute;

namespace MedControl.Application.Tests.Doctors;

public sealed class UpdateDoctorCommandHandlerTests
{
    private readonly IDoctorRepository _doctorRepository = Substitute.For<IDoctorRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentTenantService _currentTenant = Substitute.For<ICurrentTenantService>();
    private readonly UpdateDoctorCommandHandler _sut;

    public UpdateDoctorCommandHandlerTests()
    {
        _sut = new UpdateDoctorCommandHandler(_doctorRepository, _unitOfWork, _currentTenant);
    }

    [Fact]
    public async Task Handle_ComDadosValidos_DeveAtualizarEPersistir()
    {
        var tenantId = Guid.NewGuid();
        var doctor = DoctorProfile.Create(tenantId, "Dr. João", "123456", "SP", "Cardiologia").Value;
        _currentTenant.TenantId.Returns(tenantId);
        _doctorRepository.GetByIdAsync(doctor.Id, Arg.Any<CancellationToken>()).Returns(doctor);
        _doctorRepository.ExistsByCrmAsync(tenantId, "654321", "RJ", Arg.Any<CancellationToken>()).Returns(false);

        var command = new UpdateDoctorCommand(doctor.Id, "Dr. Maria", "654321", "RJ", "Neurologia");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Dr. Maria");
        result.Value.Crm.Should().Be("654321");
        result.Value.CouncilState.Should().Be("RJ");
        result.Value.Specialty.Should().Be("Neurologia");
        await _doctorRepository.Received(1).UpdateAsync(doctor, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SemTenant_DeveRetornarUnauthorized()
    {
        _currentTenant.TenantId.Returns((Guid?)null);

        var command = new UpdateDoctorCommand(Guid.NewGuid(), "Dr. João", "123456", "SP", "Cardiologia");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_MedicoNaoEncontrado_DeveRetornarNotFound()
    {
        var tenantId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        _currentTenant.TenantId.Returns(tenantId);
        _doctorRepository.GetByIdAsync(doctorId, Arg.Any<CancellationToken>()).Returns((DoctorProfile?)null);

        var command = new UpdateDoctorCommand(doctorId, "Dr. João", "123456", "SP", "Cardiologia");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        await _doctorRepository.DidNotReceive().UpdateAsync(Arg.Any<DoctorProfile>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CrmDuplicadoEmOutroMedico_DeveRetornarConflict()
    {
        var tenantId = Guid.NewGuid();
        var doctor = DoctorProfile.Create(tenantId, "Dr. João", "123456", "SP", "Cardiologia").Value;
        _currentTenant.TenantId.Returns(tenantId);
        _doctorRepository.GetByIdAsync(doctor.Id, Arg.Any<CancellationToken>()).Returns(doctor);
        _doctorRepository.ExistsByCrmAsync(tenantId, "999999", "SP", Arg.Any<CancellationToken>()).Returns(true);

        var command = new UpdateDoctorCommand(doctor.Id, "Dr. João", "999999", "SP", "Cardiologia");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        await _doctorRepository.DidNotReceive().UpdateAsync(Arg.Any<DoctorProfile>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MesmoCrmDoProprioMedico_DevePermitirAtualizar()
    {
        var tenantId = Guid.NewGuid();
        var doctor = DoctorProfile.Create(tenantId, "Dr. João", "123456", "SP", "Cardiologia").Value;
        _currentTenant.TenantId.Returns(tenantId);
        _doctorRepository.GetByIdAsync(doctor.Id, Arg.Any<CancellationToken>()).Returns(doctor);
        // CRM não muda — ExistsByCrm não é chamado para o próprio médico
        _doctorRepository.ExistsByCrmAsync(tenantId, "123456", "SP", Arg.Any<CancellationToken>()).Returns(false);

        var command = new UpdateDoctorCommand(doctor.Id, "Dr. João Atualizado", "123456", "SP", "Neurologia");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Validator_ComIdVazio_DeveRetornarErroDeValidacao()
    {
        var validator = new UpdateDoctorCommandValidator();
        var validation = await validator.ValidateAsync(
            new UpdateDoctorCommand(Guid.Empty, "Dr. João", "123456", "SP", "Cardiologia"));
        validation.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_ComNomeVazio_DeveRetornarErroDeValidacao()
    {
        var validator = new UpdateDoctorCommandValidator();
        var validation = await validator.ValidateAsync(
            new UpdateDoctorCommand(Guid.NewGuid(), string.Empty, "123456", "SP", "Cardiologia"));
        validation.IsValid.Should().BeFalse();
    }
}
