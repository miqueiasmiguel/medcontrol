using FluentValidation.TestHelper;
using MedControl.Application.Auth.Commands.VerifyMagicLink;

namespace MedControl.Application.Tests.Auth;

public sealed class VerifyMagicLinkCommandValidatorTests
{
    private readonly VerifyMagicLinkCommandValidator _sut = new();

    [Fact]
    public void Should_pass_when_token_is_provided()
    {
        var result = _sut.TestValidate(new VerifyMagicLinkCommand("some-valid-token"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void Should_fail_when_token_is_empty(string? token)
    {
        var result = _sut.TestValidate(new VerifyMagicLinkCommand(token!));
        result.ShouldHaveValidationErrorFor(x => x.Token);
    }
}
