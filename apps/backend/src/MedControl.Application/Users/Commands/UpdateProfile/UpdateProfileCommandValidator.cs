using FluentValidation;

namespace MedControl.Application.Users.Commands.UpdateProfile;

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.DisplayName)
            .MaximumLength(100)
            .When(x => x.DisplayName is not null);
    }
}
