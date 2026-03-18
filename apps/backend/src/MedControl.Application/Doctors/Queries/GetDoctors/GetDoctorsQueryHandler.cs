using MedControl.Application.Common.Interfaces;
using MedControl.Application.Doctors.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;
using MedControl.Domain.Doctors;

namespace MedControl.Application.Doctors.Queries.GetDoctors;

public sealed class GetDoctorsQueryHandler(
    IDoctorRepository doctorRepository,
    ICurrentTenantService currentTenant)
    : IRequestHandler<GetDoctorsQuery, Result<IReadOnlyList<DoctorDto>>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Doctor.Unauthorized", "A tenant context is required.");

    public async Task<Result<IReadOnlyList<DoctorDto>>> Handle(GetDoctorsQuery request, CancellationToken ct)
    {
        if (currentTenant.TenantId is null)
        {
            return Result.Failure<IReadOnlyList<DoctorDto>>(Unauthorized);
        }

        var doctors = await doctorRepository.ListAsync(ct);

        var dtos = doctors
            .Select(d => new DoctorDto(d.Id, d.TenantId, d.UserId, d.Name, d.Crm, d.CouncilState, d.Specialty))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<DoctorDto>>(dtos);
    }
}
