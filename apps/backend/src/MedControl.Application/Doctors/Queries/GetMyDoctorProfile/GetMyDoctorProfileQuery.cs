using MedControl.Application.Doctors.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Application.Doctors.Queries.GetMyDoctorProfile;

public record GetMyDoctorProfileQuery : IQuery<Result<DoctorDto?>>;
