using MedControl.Application.Doctors.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Application.Doctors.Commands.InviteAndLinkMemberToDoctor;

public record InviteAndLinkMemberToDoctorCommand(
    Guid DoctorId,
    string Email) : ICommand<Result<DoctorDto>>;
