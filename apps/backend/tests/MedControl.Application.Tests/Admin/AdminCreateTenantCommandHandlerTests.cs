using FluentAssertions;
using MedControl.Application.Admin.Commands.CreateTenant;
using MedControl.Application.Auth.Settings;
using MedControl.Application.Common.Interfaces;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;
using MedControl.Domain.Users;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace MedControl.Application.Tests.Admin;

public sealed class AdminCreateTenantCommandHandlerTests
{
    private readonly ITenantRepository _tenantRepository = Substitute.For<ITenantRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IEmailService _emailService = Substitute.For<IEmailService>();
    private readonly IMagicLinkService _magicLinkService = Substitute.For<IMagicLinkService>();
    private readonly IOptions<MagicLinkSettings> _magicLinkSettings =
        Options.Create(new MagicLinkSettings { BaseUrl = "https://app.medcontrol.test/auth/verify", TokenExpiryMinutes = 15 });
    private readonly AdminCreateTenantCommandHandler _sut;

    public AdminCreateTenantCommandHandlerTests()
    {
        _sut = new AdminCreateTenantCommandHandler(
            _tenantRepository, _userRepository, _unitOfWork, _currentUser,
            _emailService, _magicLinkService, _magicLinkSettings);
    }

    [Fact]
    public async Task Handle_UsuarioSemGlobalAdmin_DeveRetornarUnauthorized()
    {
        _currentUser.HasGlobalRole("admin").Returns(false);

        var result = await _sut.Handle(
            new AdminCreateTenantCommand("Clinic", "owner@example.com"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_NomeInvalido_DeveRetornarFailure()
    {
        _currentUser.HasGlobalRole("admin").Returns(true);

        var result = await _sut.Handle(
            new AdminCreateTenantCommand("", "owner@example.com"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_OwnerEmailExistente_DeveCriarTenantAdicionarOwnerSemConvite()
    {
        var existingUser = User.Create("owner@example.com", "Owner").Value;
        _currentUser.HasGlobalRole("admin").Returns(true);
        _userRepository.GetByEmailAsync("owner@example.com", Arg.Any<CancellationToken>())
            .Returns(existingUser);

        var result = await _sut.Handle(
            new AdminCreateTenantCommand("Clinic A", "owner@example.com"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Clinic A");
        result.Value.MemberCount.Should().Be(1);
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _emailService.DidNotReceive()
            .SendInvitationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _tenantRepository.Received(1).AddAsync(Arg.Any<Tenant>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OwnerEmailNovo_DeveCriarUsuarioCriarTenantEEnviarConvite()
    {
        _currentUser.HasGlobalRole("admin").Returns(true);
        _userRepository.GetByEmailAsync("new@example.com", Arg.Any<CancellationToken>())
            .ReturnsNull();
        _magicLinkService.GenerateInviteTokenAsync("new@example.com", Arg.Any<CancellationToken>())
            .Returns("invite-token");

        var result = await _sut.Handle(
            new AdminCreateTenantCommand("New Clinic", "new@example.com"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New Clinic");
        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u => u.Email == "new@example.com"),
            Arg.Any<CancellationToken>());
        await _emailService.Received(1).SendInvitationAsync(
            "new@example.com",
            Arg.Is<string>(url => url.Contains("invite-token")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DadosValidos_OwnerEhAdicionadoComRoleOwner()
    {
        var existingUser = User.Create("owner@example.com").Value;
        _currentUser.HasGlobalRole("admin").Returns(true);
        _userRepository.GetByEmailAsync("owner@example.com", Arg.Any<CancellationToken>())
            .Returns(existingUser);

        Tenant? capturedTenant = null;
        await _tenantRepository.AddAsync(
            Arg.Do<Tenant>(t => capturedTenant = t),
            Arg.Any<CancellationToken>());

        var result = await _sut.Handle(
            new AdminCreateTenantCommand("Clinic B", "owner@example.com"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        capturedTenant.Should().NotBeNull();
        capturedTenant!.Members.Should().HaveCount(1);
        capturedTenant.Members[0].Role.Should().Be(TenantRole.Owner);
        capturedTenant.Members[0].UserId.Should().Be(existingUser.Id);
    }
}
