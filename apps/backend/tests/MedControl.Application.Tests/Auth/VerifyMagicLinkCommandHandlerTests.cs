using FluentAssertions;
using MedControl.Application.Auth.Commands.VerifyMagicLink;
using MedControl.Application.Common.Interfaces;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;
using MedControl.Domain.Users;
using NSubstitute;

namespace MedControl.Application.Tests.Auth;

public sealed class VerifyMagicLinkCommandHandlerTests
{
    private readonly IMagicLinkService _magicLinkService = Substitute.For<IMagicLinkService>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ITenantRepository _tenantRepository = Substitute.For<ITenantRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();

    private readonly VerifyMagicLinkCommandHandler _sut;

    public VerifyMagicLinkCommandHandlerTests()
    {
        _sut = new VerifyMagicLinkCommandHandler(
            _magicLinkService, _userRepository, _tenantRepository, _unitOfWork, _tokenService);
    }

    [Fact]
    public async Task Handle_InvalidToken_ReturnsUnauthorized()
    {
        _magicLinkService.ValidateTokenAsync("bad-token").Returns((string?)null);

        var result = await _sut.Handle(
            new VerifyMagicLinkCommand("bad-token"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_ValidTokenButUserNotFound_ReturnsNotFound()
    {
        _magicLinkService.ValidateTokenAsync("tok").Returns("ghost@example.com");
        _userRepository.GetByEmailAsync("ghost@example.com").Returns((User?)null);

        var result = await _sut.Handle(
            new VerifyMagicLinkCommand("tok"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ValidToken_ReturnsAuthTokenDto()
    {
        var email = "user@example.com";
        var user = User.Create(email).Value;
        var tokenPair = new TokenPair("access", "refresh", DateTimeOffset.UtcNow.AddHours(1));

        _magicLinkService.ValidateTokenAsync("tok").Returns(email);
        _userRepository.GetByEmailAsync(email).Returns(user);
        _tenantRepository.ListByUserAsync(user.Id).Returns([]);
        _tokenService.GenerateTokenPair(
            user.Id, email, null,
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<IReadOnlyList<string>>())
            .Returns(tokenPair);

        var result = await _sut.Handle(
            new VerifyMagicLinkCommand("tok"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access");
        result.Value.RefreshToken.Should().Be("refresh");
    }

    [Fact]
    public async Task Handle_ValidToken_VerifiesEmailAndRecordsLogin()
    {
        var email = "user@example.com";
        var user = User.Create(email).Value;
        user.IsEmailVerified.Should().BeFalse();

        _magicLinkService.ValidateTokenAsync("tok").Returns(email);
        _userRepository.GetByEmailAsync(email).Returns(user);
        _tenantRepository.ListByUserAsync(user.Id).Returns([]);
        _tokenService.GenerateTokenPair(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<Guid?>(),
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<IReadOnlyList<string>>())
            .Returns(new TokenPair("a", "r", DateTimeOffset.UtcNow));

        await _sut.Handle(new VerifyMagicLinkCommand("tok"), CancellationToken.None);

        user.IsEmailVerified.Should().BeTrue();
        user.LastLoginAt.Should().NotBeNull();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UserWithTenant_IncludesTenantIdAndRoleInToken()
    {
        // Arrange — bug: magic link não incluía tenant no JWT, causando lista de pagamentos vazia
        var email = "doctor@example.com";
        var user = User.Create(email).Value;

        var tenant = Tenant.Create("Clínica Exemplo").Value;
        tenant.AddMember(user.Id, TenantRole.Doctor);

        var tokenPair = new TokenPair("access", "refresh", DateTimeOffset.UtcNow.AddHours(1));

        _magicLinkService.ValidateTokenAsync("tok").Returns(email);
        _userRepository.GetByEmailAsync(email).Returns(user);
        _tenantRepository.ListByUserAsync(user.Id).Returns([tenant]);
        _tokenService.GenerateTokenPair(
            user.Id, email, tenant.Id,
            Arg.Is<IReadOnlyList<string>>(r => r.Contains("doctor")),
            Arg.Any<IReadOnlyList<string>>())
            .Returns(tokenPair);

        // Act
        var result = await _sut.Handle(new VerifyMagicLinkCommand("tok"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _tokenService.Received(1).GenerateTokenPair(
            user.Id, email, tenant.Id,
            Arg.Is<IReadOnlyList<string>>(r => r.Contains("doctor")),
            Arg.Any<IReadOnlyList<string>>());
    }

    [Fact]
    public async Task Handle_UserWithNoTenants_GeneratesTokenWithNullTenantId()
    {
        var email = "newuser@example.com";
        var user = User.Create(email).Value;
        var tokenPair = new TokenPair("access", "refresh", DateTimeOffset.UtcNow.AddHours(1));

        _magicLinkService.ValidateTokenAsync("tok").Returns(email);
        _userRepository.GetByEmailAsync(email).Returns(user);
        _tenantRepository.ListByUserAsync(user.Id).Returns([]);
        _tokenService.GenerateTokenPair(
            user.Id, email, null,
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<IReadOnlyList<string>>())
            .Returns(tokenPair);

        var result = await _sut.Handle(new VerifyMagicLinkCommand("tok"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _tokenService.Received(1).GenerateTokenPair(
            user.Id, email, (Guid?)null,
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<IReadOnlyList<string>>());
    }

    [Fact]
    public async Task Handle_UsuarioComTenantInativo_RetornaTenantDisabled()
    {
        var email = "user@example.com";
        var user = User.Create(email).Value;
        var tenant = Tenant.Create("Clinic").Value;
        tenant.AddMember(user.Id, TenantRole.Operator);
        tenant.Deactivate();

        _magicLinkService.ValidateTokenAsync("tok").Returns(email);
        _userRepository.GetByEmailAsync(email).Returns(user);
        _tenantRepository.ListByUserAsync(user.Id).Returns([tenant]);

        var result = await _sut.Handle(new VerifyMagicLinkCommand("tok"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.TenantDisabled");
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
        _tokenService.DidNotReceive().GenerateTokenPair(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<Guid?>(),
            Arg.Any<IReadOnlyList<string>>(), Arg.Any<IReadOnlyList<string>>());
    }

    [Fact]
    public async Task Handle_UsuarioComMultiplosTenants_UsaApenasAtivos()
    {
        var email = "user@example.com";
        var user = User.Create(email).Value;
        var inactiveTenant = Tenant.Create("Inactive Clinic").Value;
        inactiveTenant.AddMember(user.Id, TenantRole.Operator);
        inactiveTenant.Deactivate();
        var activeTenant = Tenant.Create("Active Clinic").Value;
        activeTenant.AddMember(user.Id, TenantRole.Admin);
        var tokenPair = new TokenPair("access", "refresh", DateTimeOffset.UtcNow.AddHours(1));

        _magicLinkService.ValidateTokenAsync("tok").Returns(email);
        _userRepository.GetByEmailAsync(email).Returns(user);
        _tenantRepository.ListByUserAsync(user.Id).Returns([inactiveTenant, activeTenant]);
        _tokenService.GenerateTokenPair(
            user.Id, email, activeTenant.Id,
            Arg.Is<IReadOnlyList<string>>(r => r.Contains("admin")),
            Arg.Any<IReadOnlyList<string>>())
            .Returns(tokenPair);

        var result = await _sut.Handle(new VerifyMagicLinkCommand("tok"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _tokenService.Received(1).GenerateTokenPair(
            user.Id, email, activeTenant.Id,
            Arg.Is<IReadOnlyList<string>>(r => r.Contains("admin")),
            Arg.Any<IReadOnlyList<string>>());
    }

    [Fact]
    public async Task Handle_TodosTenantInativos_RetornaTenantDisabled()
    {
        var email = "user@example.com";
        var user = User.Create(email).Value;
        var t1 = Tenant.Create("Clinic A").Value;
        t1.AddMember(user.Id, TenantRole.Operator);
        t1.Deactivate();
        var t2 = Tenant.Create("Clinic B").Value;
        t2.AddMember(user.Id, TenantRole.Doctor);
        t2.Deactivate();

        _magicLinkService.ValidateTokenAsync("tok").Returns(email);
        _userRepository.GetByEmailAsync(email).Returns(user);
        _tenantRepository.ListByUserAsync(user.Id).Returns([t1, t2]);

        var result = await _sut.Handle(new VerifyMagicLinkCommand("tok"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.TenantDisabled");
    }
}
