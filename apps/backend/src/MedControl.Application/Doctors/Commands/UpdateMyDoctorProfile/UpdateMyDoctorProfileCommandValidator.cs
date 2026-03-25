using FluentValidation;

namespace MedControl.Application.Doctors.Commands.UpdateMyDoctorProfile;

public sealed class UpdateMyDoctorProfileCommandValidator : AbstractValidator<UpdateMyDoctorProfileCommand>
{
    public UpdateMyDoctorProfileCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Crm).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CouncilState).NotEmpty().MaximumLength(2);
        RuleFor(x => x.Specialty).NotEmpty().MaximumLength(256);
    }
}
