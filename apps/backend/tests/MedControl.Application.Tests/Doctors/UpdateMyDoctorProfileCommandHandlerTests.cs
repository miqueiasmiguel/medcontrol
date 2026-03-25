using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Doctors.Commands.UpdateMyDoctorProfile;
using MedControl.Domain.Common;
using MedControl.Domain.Doctors;
using NSubstitute;

namespace MedControl.Application.Tests.Doctors;

public sealed class UpdateMyDoctorProfileCommandHandlerTests
{
    private readonly IDoctorRepository _doctorRepository = Substitute.For<IDoctorRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly UpdateMyDoctorProfileCommandHandler _sut;

    public UpdateMyDoctorProfileCommandHandlerTests()
    {
        _sut = new UpdateMyDoctorProfileCommandHandler(_doctorRepository, _unitOfWork, _currentUser);
    }

    [Fact]
    public async Task Handle_UsuarioNaoAutenticado_DeveRetornarUnauthorized()
    {
        _currentUser.UserId.Returns((Guid?)null);

        var command = new UpdateMyDoctorProfileCommand("Dr. João", "123456", "SP", "Cardiologia");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_SemPerfisVinculados_DeveRetornarNotFound()
    {
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId);
        _doctorRepository.GetAllByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns([]);

        var command = new UpdateMyDoctorProfileCommand("Dr. João", "123456", "SP", "Cardiologia");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DadosValidos_DeveAtualizarTodosOsPerfis()
    {
        var userId = Guid.NewGuid();
        var tenant1 = Guid.NewGuid();
        var tenant2 = Guid.NewGuid();
        var doctor1 = DoctorProfile.Create(tenant1, "Dr. João", "123456", "SP", "Cardiologia").Value;
        var doctor2 = DoctorProfile.Create(tenant2, "Dr. João", "123456", "SP", "Cardiologia").Value;
        doctor1.LinkUser(userId);
        doctor2.LinkUser(userId);
        _currentUser.UserId.Returns(userId);
        _doctorRepository.GetAllByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<DoctorProfile> { doctor1, doctor2 });
        _doctorRepository.ExistsByCrmInTenantAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new UpdateMyDoctorProfileCommand("Dr. João Atualizado", "654321", "RJ", "Neurologia");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(d => d.Name.Should().Be("Dr. João Atualizado"));
        result.Value.Should().AllSatisfy(d => d.Crm.Should().Be("654321"));
        await _doctorRepository.Received(2).UpdateAsync(Arg.Any<DoctorProfile>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MesmoCrmSemMudanca_NaoDeveVerificarDuplicidade()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var doctor = DoctorProfile.Create(tenantId, "Dr. João", "123456", "SP", "Cardiologia").Value;
        doctor.LinkUser(userId);
        _currentUser.UserId.Returns(userId);
        _doctorRepository.GetAllByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<DoctorProfile> { doctor });

        var command = new UpdateMyDoctorProfileCommand("Dr. João Atualizado", "123456", "SP", "Neurologia");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _doctorRepository.DidNotReceive()
            .ExistsByCrmInTenantAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CrmDuplicadoEmTenant_DeveRetornarConflict()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var doctor = DoctorProfile.Create(tenantId, "Dr. João", "123456", "SP", "Cardiologia").Value;
        doctor.LinkUser(userId);
        _currentUser.UserId.Returns(userId);
        _doctorRepository.GetAllByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<DoctorProfile> { doctor });
        _doctorRepository.ExistsByCrmInTenantAsync(tenantId, "999999", "RJ", doctor.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new UpdateMyDoctorProfileCommand("Dr. João", "999999", "RJ", "Cardiologia");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public sealed class UpdateMyDoctorProfileCommandValidatorTests
{
    private readonly UpdateMyDoctorProfileCommandValidator _validator = new();

    [Theory]
    [InlineData("", "123456", "SP", "Cardiologia")]
    [InlineData("Dr. João", "", "SP", "Cardiologia")]
    [InlineData("Dr. João", "123456", "", "Cardiologia")]
    [InlineData("Dr. João", "123456", "SP", "")]
    [InlineData("Dr. João", "123456", "SPX", "Cardiologia")]
    public async Task Validator_CamposInvalidos_DeveRetornarErros(string name, string crm, string councilState, string specialty)
    {
        var validation = await _validator.ValidateAsync(
            new UpdateMyDoctorProfileCommand(name, crm, councilState, specialty));
        validation.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_CamposValidos_DeveRetornarSucesso()
    {
        var validation = await _validator.ValidateAsync(
            new UpdateMyDoctorProfileCommand("Dr. João", "123456", "SP", "Cardiologia"));
        validation.IsValid.Should().BeTrue();
    }
}
