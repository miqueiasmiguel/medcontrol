using MedControl.Application.Common.Interfaces;
using MedControl.Application.Doctors.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;
using MedControl.Domain.Doctors;

namespace MedControl.Application.Doctors.Commands.UpdateDoctor;

public sealed class UpdateDoctorCommandHandler(
    IDoctorRepository doctorRepository,
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenant)
    : IRequestHandler<UpdateDoctorCommand, Result<DoctorDto>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Doctor.Unauthorized", "A tenant context is required.");

    private static readonly Error NotFound =
        Error.NotFound("Doctor.NotFound", "Doctor not found.");

    private static readonly Error CrmAlreadyExists =
        Error.Conflict("Doctor.CrmAlreadyExists", "A doctor with this CRM and council state already exists in this tenant.");

    public async Task<Result<DoctorDto>> Handle(UpdateDoctorCommand request, CancellationToken ct)
    {
        if (currentTenant.TenantId is null)
        {
            return Result.Failure<DoctorDto>(Unauthorized);
        }

        var tenantId = currentTenant.TenantId.Value;

        var doctor = await doctorRepository.GetByIdAsync(request.Id, ct);
        if (doctor is null)
        {
            return Result.Failure<DoctorDto>(NotFound);
        }

        var crmChanged = doctor.Crm != request.Crm.Trim() || doctor.CouncilState != request.CouncilState.Trim();
        if (crmChanged)
        {
            var alreadyExists = await doctorRepository.ExistsByCrmAsync(tenantId, request.Crm, request.CouncilState, ct);
            if (alreadyExists)
            {
                return Result.Failure<DoctorDto>(CrmAlreadyExists);
            }
        }

        var updateResult = doctor.Update(request.Name, request.Crm, request.CouncilState, request.Specialty);
        if (updateResult.IsFailure)
        {
            return Result.Failure<DoctorDto>(updateResult.Error);
        }

        await doctorRepository.UpdateAsync(doctor, ct);
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
