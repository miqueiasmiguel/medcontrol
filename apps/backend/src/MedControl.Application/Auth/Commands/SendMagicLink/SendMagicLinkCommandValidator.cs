using FluentValidation;

namespace MedControl.Application.Auth.Commands.SendMagicLink;

public sealed class SendMagicLinkCommandValidator : AbstractValidator<SendMagicLinkCommand>
{
    public SendMagicLinkCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);
    }
}
