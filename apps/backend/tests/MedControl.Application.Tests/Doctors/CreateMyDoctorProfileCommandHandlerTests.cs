using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Doctors.Commands.CreateMyDoctorProfile;
using MedControl.Domain.Common;
using MedControl.Domain.Doctors;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace MedControl.Application.Tests.Doctors;

public sealed class CreateMyDoctorProfileCommandHandlerTests
{
    private readonly IDoctorRepository _doctorRepository = Substitute.For<IDoctorRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentTenantService _currentTenant = Substitute.For<ICurrentTenantService>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly CreateMyDoctorProfileCommandHandler _sut;

    public CreateMyDoctorProfileCommandHandlerTests()
    {
        _currentUser.Roles.Returns(new List<string> { "doctor" });
        _sut = new CreateMyDoctorProfileCommandHandler(
            _doctorRepository, _unitOfWork, _currentTenant, _currentUser);
    }

    [Fact]
    public async Task Handle_SemAutenticacao_DeveRetornarUnauthorized()
    {
        _currentUser.UserId.Returns((Guid?)null);
        _currentTenant.TenantId.Returns(Guid.NewGuid());

        var result = await _sut.Handle(
            new CreateMyDoctorProfileCommand("Dr. João", "123456", "SP", "Cardiologia"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_SemTenant_DeveRetornarUnauthorized()
    {
        _currentUser.UserId.Returns(Guid.NewGuid());
        _currentTenant.TenantId.Returns((Guid?)null);

        var result = await _sut.Handle(
            new CreateMyDoctorProfileCommand("Dr. João", "123456", "SP", "Cardiologia"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_UsuarioNaoTemRoleDoctor_DeveRetornarUnauthorized()
    {
        _currentUser.UserId.Returns(Guid.NewGuid());
        _currentTenant.TenantId.Returns(Guid.NewGuid());
        _currentUser.Roles.Returns(new List<string> { "operator" });

        var result = await _sut.Handle(
            new CreateMyDoctorProfileCommand("Dr. João", "123456", "SP", "Cardiologia"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_CrmDuplicado_DeveRetornarConflict()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId);
        _currentTenant.TenantId.Returns(tenantId);
        _doctorRepository.ExistsByCrmAsync(tenantId, "123456", "SP", Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.Handle(
            new CreateMyDoctorProfileCommand("Dr. João", "123456", "SP", "Cardiologia"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        await _doctorRepository.DidNotReceive().AddAsync(Arg.Any<DoctorProfile>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PerfilJaExisteParaEsteUsuario_DeveRetornarConflict()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existingProfile = DoctorProfile.Create(tenantId, "Dr. João", "123456", "SP", "Cardiologia").Value;
        existingProfile.LinkUser(userId);
        _currentUser.UserId.Returns(userId);
        _currentTenant.TenantId.Returns(tenantId);
        _doctorRepository.ExistsByCrmAsync(tenantId, "654321", "RJ", Arg.Any<CancellationToken>()).Returns(false);
        _doctorRepository.GetByCurrentUserAsync(userId, Arg.Any<CancellationToken>()).Returns(existingProfile);

        var result = await _sut.Handle(
            new CreateMyDoctorProfileCommand("Dr. João", "654321", "RJ", "Neurologia"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Handle_ComDadosValidos_DeveCriarPerfilVinculadoAoUsuario()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId);
        _currentTenant.TenantId.Returns(tenantId);
        _doctorRepository.ExistsByCrmAsync(tenantId, "123456", "SP", Arg.Any<CancellationToken>()).Returns(false);
        _doctorRepository.GetByCurrentUserAsync(userId, Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _sut.Handle(
            new CreateMyDoctorProfileCommand("Dr. João", "123456", "SP", "Cardiologia"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(userId);
        result.Value.Name.Should().Be("Dr. João");
        result.Value.Crm.Should().Be("123456");
        await _doctorRepository.Received(1).AddAsync(
            Arg.Is<DoctorProfile>(d => d.UserId == userId && d.TenantId == tenantId),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Validator_ComNomeVazio_DeveRetornarErro()
    {
        var validator = new CreateMyDoctorProfileCommandValidator();
        var validation = await validator.ValidateAsync(
            new CreateMyDoctorProfileCommand(string.Empty, "123456", "SP", "Cardiologia"));
        validation.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_ComCrmVazio_DeveRetornarErro()
    {
        var validator = new CreateMyDoctorProfileCommandValidator();
        var validation = await validator.ValidateAsync(
            new CreateMyDoctorProfileCommand("Dr. João", string.Empty, "SP", "Cardiologia"));
        validation.IsValid.Should().BeFalse();
    }
}
