using FluentAssertions;
using MedControl.Application.Auth.Commands.SendMagicLink;
using MedControl.Application.Auth.Settings;
using MedControl.Application.Common.Interfaces;
using MedControl.Domain.Users;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace MedControl.Application.Tests.Auth;

public sealed class SendMagicLinkCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMagicLinkService _magicLinkService = Substitute.For<IMagicLinkService>();
    private readonly IEmailService _emailService = Substitute.For<IEmailService>();
    private readonly IOptions<MagicLinkSettings> _settings = Options.Create(new MagicLinkSettings
    {
        BaseUrl = "https://app.example.com/auth/verify",
        TokenExpiryMinutes = 15,
    });

    private readonly SendMagicLinkCommandHandler _sut;

    public SendMagicLinkCommandHandlerTests()
    {
        _sut = new SendMagicLinkCommandHandler(
            _userRepository, _unitOfWork, _magicLinkService, _emailService, _settings);
    }

    [Fact]
    public async Task Handle_ExistingUser_SendsMagicLink()
    {
        var email = "user@example.com";
        var existingUser = User.Create(email).Value;
        _userRepository.GetByEmailAsync(email).Returns(existingUser);
        _magicLinkService.GenerateTokenAsync(email).Returns("tok123");

        await _sut.Handle(new SendMagicLinkCommand(email), CancellationToken.None);

        await _emailService.Received(1).SendMagicLinkAsync(
            email,
            "https://app.example.com/auth/verify?token=tok123",
            Arg.Any<CancellationToken>());
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NewUser_CreatesUserAndSendsMagicLink()
    {
        var email = "new@example.com";
        _userRepository.GetByEmailAsync(email).Returns((User?)null);
        _magicLinkService.GenerateTokenAsync(email).Returns("newtoken");

        await _sut.Handle(new SendMagicLinkCommand(email), CancellationToken.None);

        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u => u.Email == email),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _emailService.Received(1).SendMagicLinkAsync(
            email,
            "https://app.example.com/auth/verify?token=newtoken",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NormalizesEmailToLowercase()
    {
        var rawEmail = "User@Example.COM";
        var normalized = "user@example.com";
        _userRepository.GetByEmailAsync(normalized).Returns((User?)null);
        _magicLinkService.GenerateTokenAsync(normalized).Returns("t");

        await _sut.Handle(new SendMagicLinkCommand(rawEmail), CancellationToken.None);

        await _userRepository.Received(1).GetByEmailAsync(normalized, Arg.Any<CancellationToken>());
    }
}
