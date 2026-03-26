using FluentValidation;

namespace MedControl.Application.Doctors.Commands.InviteAndLinkMemberToDoctor;

public sealed class InviteAndLinkMemberToDoctorCommandValidator : AbstractValidator<InviteAndLinkMemberToDoctorCommand>
{
    public InviteAndLinkMemberToDoctorCommandValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
