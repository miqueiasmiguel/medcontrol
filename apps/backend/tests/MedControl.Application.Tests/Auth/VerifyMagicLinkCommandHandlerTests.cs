using FluentAssertions;
using MedControl.Application.Auth.Commands.VerifyMagicLink;
using MedControl.Application.Common.Interfaces;
using MedControl.Domain.Common;
using MedControl.Domain.Users;
using NSubstitute;

namespace MedControl.Application.Tests.Auth;

public sealed class VerifyMagicLinkCommandHandlerTests
{
    private readonly IMagicLinkService _magicLinkService = Substitute.For<IMagicLinkService>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();

    private readonly VerifyMagicLinkCommandHandler _sut;

    public VerifyMagicLinkCommandHandlerTests()
    {
        _sut = new VerifyMagicLinkCommandHandler(
            _magicLinkService, _userRepository, _unitOfWork, _tokenService);
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
}
