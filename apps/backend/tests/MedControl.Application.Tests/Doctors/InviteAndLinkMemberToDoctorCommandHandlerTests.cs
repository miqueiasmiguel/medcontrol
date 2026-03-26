using FluentAssertions;
using MedControl.Application.Auth.Settings;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Doctors.Commands.InviteAndLinkMemberToDoctor;
using MedControl.Domain.Common;
using MedControl.Domain.Doctors;
using MedControl.Domain.Tenants;
using MedControl.Domain.Users;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace MedControl.Application.Tests.Doctors;

public sealed class InviteAndLinkMemberToDoctorCommandHandlerTests
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
    private readonly InviteAndLinkMemberToDoctorCommandHandler _sut;

    public InviteAndLinkMemberToDoctorCommandHandlerTests()
    {
        _currentUser.Roles.Returns(new List<string> { "admin" });
        _sut = new InviteAndLinkMemberToDoctorCommandHandler(
            _doctorRepository, _tenantRepository, _userRepository, _unitOfWork,
            _currentTenant, _currentUser, _emailService, _magicLinkService, _magicLinkSettings);
    }

    [Fact]
    public async Task Handle_SemTenant_DeveRetornarUnauthorized()
    {
        _currentTenant.TenantId.Returns((Guid?)null);

        var result = await _sut.Handle(
            new InviteAndLinkMemberToDoctorCommand(Guid.NewGuid(), "medico@clinica.com"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_UsuarioSemPermissao_DeveRetornarUnauthorized()
    {
        _currentTenant.TenantId.Returns(Guid.NewGuid());
        _currentUser.Roles.Returns(new List<string> { "operator" });

        var result = await _sut.Handle(
            new InviteAndLinkMemberToDoctorCommand(Guid.NewGuid(), "medico@clinica.com"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_DoctorNaoEncontrado_DeveRetornarNotFound()
    {
        _currentTenant.TenantId.Returns(Guid.NewGuid());
        _doctorRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _sut.Handle(
            new InviteAndLinkMemberToDoctorCommand(Guid.NewGuid(), "medico@clinica.com"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_DoctorJaVinculado_DeveRetornarConflict()
    {
        var tenantId = Guid.NewGuid();
        var doctor = DoctorProfile.Create(tenantId, "Dr. João", "123456", "SP", "Cardiologia").Value;
        doctor.LinkUser(Guid.NewGuid());
        _currentTenant.TenantId.Returns(tenantId);
        _doctorRepository.GetByIdAsync(doctor.Id, Arg.Any<CancellationToken>()).Returns(doctor);

        var result = await _sut.Handle(
            new InviteAndLinkMemberToDoctorCommand(doctor.Id, "medico@clinica.com"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmailNovoUsuario_DeveCriarMembroVincularEEnviarConvite()
    {
        var tenantId = Guid.NewGuid();
        var doctor = DoctorProfile.Create(tenantId, "Dr. João", "123456", "SP", "Cardiologia").Value;
        var tenant = Tenant.Create("Clínica").Value;
        _currentTenant.TenantId.Returns(tenantId);
        _doctorRepository.GetByIdAsync(doctor.Id, Arg.Any<CancellationToken>()).Returns(doctor);
        _userRepository.GetByEmailAsync("medico@clinica.com", Arg.Any<CancellationToken>()).ReturnsNull();
        _tenantRepository.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);
        _magicLinkService.GenerateTokenAsync("medico@clinica.com", Arg.Any<CancellationToken>()).Returns("invite-token");

        var result = await _sut.Handle(
            new InviteAndLinkMemberToDoctorCommand(doctor.Id, "medico@clinica.com"),
            CancellationToken.None);

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
    public async Task Handle_EmailUsuarioExistenteSemMembership_DeveAdicionarMembroVincularSemConvite()
    {
        var tenantId = Guid.NewGuid();
        var doctor = DoctorProfile.Create(tenantId, "Dr. João", "123456", "SP", "Cardiologia").Value;
        var existingUser = User.Create("medico@clinica.com").Value;
        var tenant = Tenant.Create("Clínica").Value;
        _currentTenant.TenantId.Returns(tenantId);
        _doctorRepository.GetByIdAsync(doctor.Id, Arg.Any<CancellationToken>()).Returns(doctor);
        _userRepository.GetByEmailAsync("medico@clinica.com", Arg.Any<CancellationToken>()).Returns(existingUser);
        _tenantRepository.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);

        var result = await _sut.Handle(
            new InviteAndLinkMemberToDoctorCommand(doctor.Id, "medico@clinica.com"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(existingUser.Id);
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _emailService.DidNotReceive().SendInvitationAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmailUsuarioJaMembroDoctor_DeveVincularSemReAdicionarMembro()
    {
        var tenantId = Guid.NewGuid();
        var doctor = DoctorProfile.Create(tenantId, "Dr. João", "123456", "SP", "Cardiologia").Value;
        var existingUser = User.Create("medico@clinica.com").Value;
        var tenant = Tenant.Create("Clínica").Value;
        tenant.AddMember(existingUser.Id, TenantRole.Doctor);
        _currentTenant.TenantId.Returns(tenantId);
        _doctorRepository.GetByIdAsync(doctor.Id, Arg.Any<CancellationToken>()).Returns(doctor);
        _userRepository.GetByEmailAsync("medico@clinica.com", Arg.Any<CancellationToken>()).Returns(existingUser);
        _tenantRepository.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);

        var result = await _sut.Handle(
            new InviteAndLinkMemberToDoctorCommand(doctor.Id, "medico@clinica.com"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(existingUser.Id);
        // tenant.AddMember not called again (already a member)
        await _tenantRepository.DidNotReceive().UpdateAsync(Arg.Any<Tenant>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OwnerTambemTemPermissao()
    {
        var tenantId = Guid.NewGuid();
        var doctor = DoctorProfile.Create(tenantId, "Dr. João", "123456", "SP", "Cardiologia").Value;
        var tenant = Tenant.Create("Clínica").Value;
        _currentTenant.TenantId.Returns(tenantId);
        _currentUser.Roles.Returns(new List<string> { "owner" });
        _doctorRepository.GetByIdAsync(doctor.Id, Arg.Any<CancellationToken>()).Returns(doctor);
        _userRepository.GetByEmailAsync("medico@clinica.com", Arg.Any<CancellationToken>()).ReturnsNull();
        _tenantRepository.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);
        _magicLinkService.GenerateTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns("token");

        var result = await _sut.Handle(
            new InviteAndLinkMemberToDoctorCommand(doctor.Id, "medico@clinica.com"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Validator_ComDoctorIdVazio_DeveRetornarErro()
    {
        var validator = new InviteAndLinkMemberToDoctorCommandValidator();
        var validation = await validator.ValidateAsync(
            new InviteAndLinkMemberToDoctorCommand(Guid.Empty, "medico@clinica.com"));
        validation.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_ComEmailVazio_DeveRetornarErro()
    {
        var validator = new InviteAndLinkMemberToDoctorCommandValidator();
        var validation = await validator.ValidateAsync(
            new InviteAndLinkMemberToDoctorCommand(Guid.NewGuid(), string.Empty));
        validation.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_ComEmailInvalido_DeveRetornarErro()
    {
        var validator = new InviteAndLinkMemberToDoctorCommandValidator();
        var validation = await validator.ValidateAsync(
            new InviteAndLinkMemberToDoctorCommand(Guid.NewGuid(), "email-invalido"));
        validation.IsValid.Should().BeFalse();
    }
}
