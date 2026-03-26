using MedControl.Application.Doctors.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Application.Doctors.Commands.LinkDoctorProfileToUser;

public record LinkDoctorProfileToUserCommand(Guid DoctorId, Guid UserId) : ICommand<Result<DoctorDto>>;
