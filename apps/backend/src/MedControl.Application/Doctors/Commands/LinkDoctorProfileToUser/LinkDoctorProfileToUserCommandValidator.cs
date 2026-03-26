using FluentValidation;

namespace MedControl.Application.Doctors.Commands.LinkDoctorProfileToUser;

public sealed class LinkDoctorProfileToUserCommandValidator : AbstractValidator<LinkDoctorProfileToUserCommand>
{
    public LinkDoctorProfileToUserCommandValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
