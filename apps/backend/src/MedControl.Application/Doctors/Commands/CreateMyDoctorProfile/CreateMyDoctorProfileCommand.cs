using MedControl.Application.Doctors.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Application.Doctors.Commands.CreateMyDoctorProfile;

public record CreateMyDoctorProfileCommand(
    string Name,
    string Crm,
    string CouncilState,
    string Specialty) : ICommand<Result<DoctorDto>>;
