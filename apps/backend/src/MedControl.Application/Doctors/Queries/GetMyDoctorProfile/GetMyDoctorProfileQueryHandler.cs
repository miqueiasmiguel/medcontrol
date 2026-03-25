using MedControl.Application.Common.Interfaces;
using MedControl.Application.Doctors.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;
using MedControl.Domain.Doctors;

namespace MedControl.Application.Doctors.Queries.GetMyDoctorProfile;

public sealed class GetMyDoctorProfileQueryHandler(
    IDoctorRepository doctorRepository,
    ICurrentUserService currentUser)
    : IRequestHandler<GetMyDoctorProfileQuery, Result<DoctorDto?>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Doctor.Unauthorized", "Authentication is required.");

    public async Task<Result<DoctorDto?>> Handle(GetMyDoctorProfileQuery request, CancellationToken ct)
    {
        if (currentUser.UserId is null)
        {
            return Result.Failure<DoctorDto?>(Unauthorized);
        }

        var doctor = await doctorRepository.GetByCurrentUserAsync(currentUser.UserId.Value, ct);
        if (doctor is null)
        {
            return Result.Success<DoctorDto?>(null);
        }

        return Result.Success<DoctorDto?>(new DoctorDto(
            doctor.Id,
            doctor.TenantId,
            doctor.UserId,
            doctor.Name,
            doctor.Crm,
            doctor.CouncilState,
            doctor.Specialty));
    }
}
