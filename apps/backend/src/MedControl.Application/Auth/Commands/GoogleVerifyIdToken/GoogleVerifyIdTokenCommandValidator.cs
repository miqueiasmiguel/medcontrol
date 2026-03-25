using FluentValidation;

namespace MedControl.Application.Auth.Commands.GoogleVerifyIdToken;

public sealed class GoogleVerifyIdTokenCommandValidator : AbstractValidator<GoogleVerifyIdTokenCommand>
{
    public GoogleVerifyIdTokenCommandValidator()
    {
        RuleFor(x => x.IdToken).NotEmpty();
    }
}
