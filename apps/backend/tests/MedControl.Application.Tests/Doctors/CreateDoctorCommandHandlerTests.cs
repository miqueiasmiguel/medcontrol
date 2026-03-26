using FluentAssertions;
using MedControl.Application.Auth.Settings;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Doctors.Commands.CreateDoctor;
using MedControl.Domain.Common;
using MedControl.Domain.Doctors;
using MedControl.Domain.Tenants;
using MedControl.Domain.Users;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace MedControl.Application.Tests.Doctors;

public sealed class CreateDoctorCommandHandlerTests
{
    private readonly IDoctorRepository _doctorRepository = Substitute.For<IDoctorRepository>();
    private readonly ITenantRepository _tenantRepository = Substitute.For<ITenantRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentTenantService _currentTenant = Substitute.For<ICurrentTenantService>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IEmailService _emailService = Substitute.For<IEmailService>();
    private readonly IMagicLinkService _magicLinkService = Substitute.For<IMagicLinkService>();
    private readonly IOptions<MagicLinkSettings> _magicLinkSettings =
        Options.Create(new MagicLinkSettings { BaseUrl = "https://app.medcontrol.com/auth", TokenExpiryMinutes = 15 });
    private readonly CreateDoctorCommandHandler _sut;

    public CreateDoctorCommandHandlerTests()
    {
        _currentUser.Roles.Returns(new List<string> { "admin" });
        _sut = new CreateDoctorCommandHandler(
            _doctorRepository, _tenantRepository, _userRepository, _unitOfWork,
            _currentTenant, _currentUser, _emailService, _magicLinkService, _magicLinkSettings);
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

    [Fact]
    public async Task Handle_ComEmailConviteNovoUsuario_DeveCriarMembroVincularEEnviarConvite()
    {
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("Clínica").Value;
        _currentTenant.TenantId.Returns(tenantId);
        _userRepository.GetByEmailAsync("medico@clinica.com", Arg.Any<CancellationToken>()).ReturnsNull();
        _tenantRepository.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);
        _doctorRepository.ExistsByCrmAsync(tenantId, "123456", "SP", Arg.Any<CancellationToken>()).Returns(false);
        _magicLinkService.GenerateTokenAsync("medico@clinica.com", Arg.Any<CancellationToken>()).Returns("invite-token");

        var command = new CreateDoctorCommand("Dr. João", "123456", "SP", "Cardiologia", "medico@clinica.com");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().NotBeNull();
        await _userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _tenantRepository.Received(1).UpdateAsync(tenant, Arg.Any<CancellationToken>());
        await _doctorRepository.Received(1).UpdateAsync(
            Arg.Is<DoctorProfile>(d => d.UserId != null),
            Arg.Any<CancellationToken>());
        await _emailService.Received(1).SendInvitationAsync(
            "medico@clinica.com",
            Arg.Is<string>(s => s.Contains("invite-token")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ComEmailConviteUsuarioExistente_DeveAdicionarMembroVincularSemEmail()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existingUser = User.Create("medico@clinica.com").Value;
        var tenant = Tenant.Create("Clínica").Value;
        _currentTenant.TenantId.Returns(tenantId);
        _userRepository.GetByEmailAsync("medico@clinica.com", Arg.Any<CancellationToken>()).Returns(existingUser);
        _tenantRepository.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);
        _doctorRepository.ExistsByCrmAsync(tenantId, "123456", "SP", Arg.Any<CancellationToken>()).Returns(false);

        var command = new CreateDoctorCommand("Dr. João", "123456", "SP", "Cardiologia", "medico@clinica.com");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(existingUser.Id);
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _emailService.DidNotReceive().SendInvitationAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ComEmailSemPermissaoAdminOwner_DeveRetornarUnauthorized()
    {
        var tenantId = Guid.NewGuid();
        _currentTenant.TenantId.Returns(tenantId);
        _currentUser.Roles.Returns(new List<string> { "operator" });
        _doctorRepository.ExistsByCrmAsync(tenantId, "123456", "SP", Arg.Any<CancellationToken>()).Returns(false);

        var command = new CreateDoctorCommand("Dr. João", "123456", "SP", "Cardiologia", "medico@clinica.com");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SemEmail_DeveCriarMedicoSemVinculo()
    {
        var tenantId = Guid.NewGuid();
        _currentTenant.TenantId.Returns(tenantId);
        _doctorRepository.ExistsByCrmAsync(tenantId, "123456", "SP", Arg.Any<CancellationToken>()).Returns(false);

        var command = new CreateDoctorCommand("Dr. João", "123456", "SP", "Cardiologia");
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().BeNull();
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _tenantRepository.DidNotReceive().UpdateAsync(Arg.Any<Tenant>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Validator_ComInviteEmailInvalido_DeveRetornarErroDeValidacao()
    {
        var validator = new CreateDoctorCommandValidator();
        var validation = await validator.ValidateAsync(
            new CreateDoctorCommand("Dr. João", "123456", "SP", "Cardiologia", "email-invalido"));
        validation.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_ComInviteEmailValido_DevePassar()
    {
        var validator = new CreateDoctorCommandValidator();
        var validation = await validator.ValidateAsync(
            new CreateDoctorCommand("Dr. João", "123456", "SP", "Cardiologia", "medico@clinica.com"));
        validation.IsValid.Should().BeTrue();
    }
}
