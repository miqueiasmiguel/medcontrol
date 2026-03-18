using MedControl.Application.Doctors.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Application.Doctors.Queries.GetDoctors;

public record GetDoctorsQuery : IQuery<Result<IReadOnlyList<DoctorDto>>>;
