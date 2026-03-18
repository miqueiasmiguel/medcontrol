using MedControl.Application.Common.Interfaces;
using MedControl.Application.Doctors.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;
using MedControl.Domain.Doctors;

namespace MedControl.Application.Doctors.Commands.CreateDoctor;

public sealed class CreateDoctorCommandHandler(
    IDoctorRepository doctorRepository,
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenant)
    : IRequestHandler<CreateDoctorCommand, Result<DoctorDto>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Doctor.Unauthorized", "A tenant context is required.");

    private static readonly Error CrmAlreadyExists =
        Error.Conflict("Doctor.CrmAlreadyExists", "A doctor with this CRM and council state already exists in this tenant.");

    public async Task<Result<DoctorDto>> Handle(CreateDoctorCommand request, CancellationToken ct)
    {
        if (currentTenant.TenantId is null)
        {
            return Result.Failure<DoctorDto>(Unauthorized);
        }

        var tenantId = currentTenant.TenantId.Value;

        var alreadyExists = await doctorRepository.ExistsByCrmAsync(tenantId, request.Crm, request.CouncilState, ct);
        if (alreadyExists)
        {
            return Result.Failure<DoctorDto>(CrmAlreadyExists);
        }

        var doctorResult = DoctorProfile.Create(tenantId, request.Name, request.Crm, request.CouncilState, request.Specialty);
        if (doctorResult.IsFailure)
        {
            return Result.Failure<DoctorDto>(doctorResult.Error);
        }

        var doctor = doctorResult.Value;
        await doctorRepository.AddAsync(doctor, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new DoctorDto(
            doctor.Id,
            doctor.TenantId,
            doctor.UserId,
            doctor.Name,
            doctor.Crm,
            doctor.CouncilState,
            doctor.Specialty));
    }
}
