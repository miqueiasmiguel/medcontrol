using MedControl.Application.Doctors.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Application.Doctors.Commands.CreateDoctor;

public record CreateDoctorCommand(
    string Name,
    string Crm,
    string CouncilState,
    string Specialty,
    string? InviteEmail = null) : ICommand<Result<DoctorDto>>;
