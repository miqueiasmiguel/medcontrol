using FluentAssertions;
using MedControl.Application.Auth.Commands.GoogleVerifyIdToken;
using MedControl.Application.Common.Interfaces;
using MedControl.Domain.Common;
using MedControl.Domain.Users;
using NSubstitute;

namespace MedControl.Application.Tests.Auth;

public sealed class GoogleVerifyIdTokenCommandHandlerTests
{
    private readonly IGoogleAuthService _googleAuthService = Substitute.For<IGoogleAuthService>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();

    private readonly GoogleVerifyIdTokenCommandHandler _sut;

    public GoogleVerifyIdTokenCommandHandlerTests()
    {
        _sut = new GoogleVerifyIdTokenCommandHandler(
            _googleAuthService, _userRepository, _unitOfWork, _tokenService);
    }

    [Fact]
    public async Task Handle_UsuarioExistente_DeveAutenticar()
    {
        var email = "user@example.com";
        var user = User.CreateFromGoogle(email, "User Name", null).Value;
        var tokenPair = new TokenPair("access", "refresh", DateTimeOffset.UtcNow.AddHours(1));
        var googleUserInfo = new GoogleUserInfo(email, "User Name", null);

        _googleAuthService.VerifyIdTokenAsync("valid-token", Arg.Any<CancellationToken>())
            .Returns(googleUserInfo);
        _userRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns(user);
        _tokenService.GenerateTokenPair(
                user.Id, email, null,
                Arg.Any<IReadOnlyList<string>>(),
                Arg.Any<IReadOnlyList<string>>())
            .Returns(tokenPair);

        var result = await _sut.Handle(
            new GoogleVerifyIdTokenCommand("valid-token"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access");
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UsuarioNaoExistente_DeveCriarEAutenticar()
    {
        var email = "new@example.com";
        var tokenPair = new TokenPair("access", "refresh", DateTimeOffset.UtcNow.AddHours(1));
        var googleUserInfo = new GoogleUserInfo(email, "New User", new Uri("https://example.com/photo.jpg"));

        _googleAuthService.VerifyIdTokenAsync("valid-token", Arg.Any<CancellationToken>())
            .Returns(googleUserInfo);
        _userRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _tokenService.GenerateTokenPair(
                Arg.Any<Guid>(), email, null,
                Arg.Any<IReadOnlyList<string>>(),
                Arg.Any<IReadOnlyList<string>>())
            .Returns(tokenPair);

        var result = await _sut.Handle(
            new GoogleVerifyIdTokenCommand("valid-token"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GoogleRetornouErro_DeveRetornarFalha()
    {
        _googleAuthService.VerifyIdTokenAsync("bad-token", Arg.Any<CancellationToken>())
            .Returns((GoogleUserInfo?)null);

        var result = await _sut.Handle(
            new GoogleVerifyIdTokenCommand("bad-token"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }
}
