using FluentValidation;

namespace MedControl.Application.Doctors.Commands.CreateDoctor;

public sealed class CreateDoctorCommandValidator : AbstractValidator<CreateDoctorCommand>
{
    public CreateDoctorCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Crm).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CouncilState).NotEmpty().MaximumLength(2);
        RuleFor(x => x.Specialty).NotEmpty().MaximumLength(256);
    }
}
