using FluentValidation;

namespace MedControl.Application.Auth.Commands.VerifyMagicLink;

public sealed class VerifyMagicLinkCommandValidator : AbstractValidator<VerifyMagicLinkCommand>
{
    public VerifyMagicLinkCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty();
    }
}
