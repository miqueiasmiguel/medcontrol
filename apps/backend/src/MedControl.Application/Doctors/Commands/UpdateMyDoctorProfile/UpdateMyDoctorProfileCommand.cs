using MedControl.Application.Doctors.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Application.Doctors.Commands.UpdateMyDoctorProfile;

public record UpdateMyDoctorProfileCommand(
    string Name,
    string Crm,
    string CouncilState,
    string Specialty)
    : ICommand<Result<IReadOnlyList<DoctorDto>>>;
