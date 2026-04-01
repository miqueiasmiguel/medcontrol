using FluentAssertions;
using MedControl.Application.Auth.Settings;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Members.Commands.AddMember;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;
using MedControl.Domain.Users;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace MedControl.Application.Tests.Members;

public sealed class AddMemberCommandHandlerTests
{
    private readonly ITenantRepository _tenantRepository = Substitute.For<ITenantRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentTenantService _currentTenant = Substitute.For<ICurrentTenantService>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IEmailService _emailService = Substitute.For<IEmailService>();
    private readonly IMagicLinkService _magicLinkService = Substitute.For<IMagicLinkService>();
    private readonly IOptions<MagicLinkSettings> _magicLinkSettings =
        Options.Create(new MagicLinkSettings { BaseUrl = "https://app.medcontrol.test/auth/verify", TokenExpiryMinutes = 15 });
    private readonly AddMemberCommandHandler _sut;

    public AddMemberCommandHandlerTests()
    {
        _sut = new AddMemberCommandHandler(
            _tenantRepository, _userRepository, _unitOfWork, _currentTenant, _currentUser,
            _emailService, _magicLinkService, _magicLinkSettings);
    }

    [Fact]
    public async Task Handle_SemTenant_DeveRetornarUnauthorized()
    {
        _currentTenant.TenantId.Returns((Guid?)null);

        var result = await _sut.Handle(new AddMemberCommand("new@example.com", "operator"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_UsuarioSemPermissao_DeveRetornarUnauthorized()
    {
        _currentTenant.TenantId.Returns(Guid.NewGuid());
        _currentUser.Roles.Returns(new List<string> { "operator" });

        var result = await _sut.Handle(new AddMemberCommand("new@example.com", "operator"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_UsuarioNaoExiste_DeveCriarUsuarioAdicionarMembroESendInvitation()
    {
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("Clinic").Value;
        _currentTenant.TenantId.Returns(tenantId);
        _currentUser.Roles.Returns(new List<string> { "admin" });
        _userRepository.GetByEmailAsync("new@example.com", Arg.Any<CancellationToken>()).ReturnsNull();
        _tenantRepository.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);
        _magicLinkService.GenerateInviteTokenAsync("new@example.com", Arg.Any<CancellationToken>()).Returns("invite-token");

        var result = await _sut.Handle(new AddMemberCommand("new@example.com", "operator"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Invited.Should().BeTrue();
        result.Value.Email.Should().Be("new@example.com");
        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u => u.Email == "new@example.com"),
            Arg.Any<CancellationToken>());
        await _magicLinkService.Received(1).GenerateInviteTokenAsync("new@example.com", Arg.Any<CancellationToken>());
        await _magicLinkService.DidNotReceive().GenerateTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _emailService.Received(1).SendInvitationAsync(
            "new@example.com",
            Arg.Is<string>(url => url.Contains("invite-token")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UsuarioExiste_NaoEnviaConviteERetornaInvitedFalse()
    {
        var tenantId = Guid.NewGuid();
        var user = User.Create("existing@example.com", "Existing").Value;
        var tenant = Tenant.Create("Clinic").Value;
        _currentTenant.TenantId.Returns(tenantId);
        _currentUser.Roles.Returns(new List<string> { "admin" });
        _userRepository.GetByEmailAsync("existing@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _tenantRepository.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);

        var result = await _sut.Handle(new AddMemberCommand("existing@example.com", "operator"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Invited.Should().BeFalse();
        await _emailService.DidNotReceive().SendInvitationAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TenantNaoEncontrado_DeveRetornarNotFound()
    {
        var tenantId = Guid.NewGuid();
        var user = User.Create("new@example.com").Value;
        _currentTenant.TenantId.Returns(tenantId);
        _currentUser.Roles.Returns(new List<string> { "admin" });
        _userRepository.GetByEmailAsync("new@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _tenantRepository.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _sut.Handle(new AddMemberCommand("new@example.com", "operator"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_UsuarioJaMembro_DeveRetornarConflict()
    {
        var tenantId = Guid.NewGuid();
        var user = User.Create("existing@example.com").Value;
        var tenant = Tenant.Create("Clinic").Value;
        tenant.AddMember(user.Id, TenantRole.Operator);

        _currentTenant.TenantId.Returns(tenantId);
        _currentUser.Roles.Returns(new List<string> { "admin" });
        _userRepository.GetByEmailAsync("existing@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _tenantRepository.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);

        var result = await _sut.Handle(new AddMemberCommand("existing@example.com", "operator"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DadosValidos_DeveAdicionarMembroERetornarDto()
    {
        var tenantId = Guid.NewGuid();
        var user = User.Create("new@example.com", "New User").Value;
        var tenant = Tenant.Create("Clinic").Value;

        _currentTenant.TenantId.Returns(tenantId);
        _currentUser.Roles.Returns(new List<string> { "admin" });
        _userRepository.GetByEmailAsync("new@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _tenantRepository.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);

        var result = await _sut.Handle(new AddMemberCommand("new@example.com", "operator"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().Be("operator");
        result.Value.Email.Should().Be("new@example.com");
        result.Value.Invited.Should().BeFalse();
        await _tenantRepository.Received(1).UpdateAsync(tenant, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Validator_EmailInvalido_DeveRetornarErro()
    {
        var validator = new AddMemberCommandValidator();
        var validation = await validator.ValidateAsync(new AddMemberCommand("not-an-email", "operator"));
        validation.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_RoleInvalida_DeveRetornarErro()
    {
        var validator = new AddMemberCommandValidator();
        var validation = await validator.ValidateAsync(new AddMemberCommand("user@example.com", "superadmin"));
        validation.IsValid.Should().BeFalse();
    }
}
