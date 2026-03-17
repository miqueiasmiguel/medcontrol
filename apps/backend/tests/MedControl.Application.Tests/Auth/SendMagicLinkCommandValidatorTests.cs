using FluentAssertions;
using FluentValidation.TestHelper;
using MedControl.Application.Auth.Commands.SendMagicLink;

namespace MedControl.Application.Tests.Auth;

public sealed class SendMagicLinkCommandValidatorTests
{
    private readonly SendMagicLinkCommandValidator _sut = new();

    [Fact]
    public void Should_pass_when_email_is_valid()
    {
        var result = _sut.TestValidate(new SendMagicLinkCommand("user@example.com"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void Should_fail_when_email_is_empty(string? email)
    {
        var result = _sut.TestValidate(new SendMagicLinkCommand(email!));
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_fail_when_email_is_invalid()
    {
        var result = _sut.TestValidate(new SendMagicLinkCommand("not-an-email"));
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_fail_when_email_exceeds_max_length()
    {
        // 251 'a' chars + "@b.com" (6 chars) = 257 chars, which exceeds MaximumLength(256)
        var result = _sut.TestValidate(new SendMagicLinkCommand(new string('a', 251) + "@b.com"));
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}
