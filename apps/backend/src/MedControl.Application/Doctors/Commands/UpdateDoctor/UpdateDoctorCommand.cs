using MedControl.Application.Doctors.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Application.Doctors.Commands.UpdateDoctor;

public record UpdateDoctorCommand(
    Guid Id,
    string Name,
    string Crm,
    string CouncilState,
    string Specialty) : ICommand<Result<DoctorDto>>;
