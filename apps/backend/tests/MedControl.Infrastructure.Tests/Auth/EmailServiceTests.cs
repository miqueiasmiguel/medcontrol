using FluentAssertions;
using MedControl.Infrastructure.Auth;
using NSubstitute;
using Resend;

namespace MedControl.Infrastructure.Tests.Auth;

public sealed class EmailServiceTests
{
    private readonly IResend _resend = Substitute.For<IResend>();
    private readonly EmailService _sut;

    public EmailServiceTests()
    {
        _sut = new EmailService(_resend);
    }

    [Fact]
    public async Task SendMagicLinkAsync_CallsResendWithCorrectRecipient()
    {
        var email = "user@example.com";
        var link = "https://app.example.com/auth/verify?token=abc123";

        await _sut.SendMagicLinkAsync(email, link);

        await _resend.Received(1).EmailSendAsync(
            Arg.Is<EmailMessage>(m => m.To.Contains(email)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendMagicLinkAsync_MessageContainsMagicLink()
    {
        var link = "https://app.example.com?token=xyz";
        EmailMessage? captured = null;
        await _resend.EmailSendAsync(
            Arg.Do<EmailMessage>(m => captured = m),
            Arg.Any<CancellationToken>());

        await _sut.SendMagicLinkAsync("u@e.com", link);

        captured?.HtmlBody.Should().Contain(link);
    }
}
