using MedControl.Application.Common.Interfaces;
using MedControl.Application.Doctors.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;
using MedControl.Domain.Doctors;

namespace MedControl.Application.Doctors.Commands.CreateMyDoctorProfile;

public sealed class CreateMyDoctorProfileCommandHandler(
    IDoctorRepository doctorRepository,
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenant,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateMyDoctorProfileCommand, Result<DoctorDto>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Doctor.Unauthorized", "Authentication with doctor role is required.");

    private static readonly Error CrmAlreadyExists =
        Error.Conflict("Doctor.CrmAlreadyExists", "A doctor with this CRM and council state already exists in this tenant.");

    private static readonly Error ProfileAlreadyExists =
        Error.Conflict("Doctor.ProfileAlreadyExists", "A doctor profile already exists for your account in this organization.");

    public async Task<Result<DoctorDto>> Handle(CreateMyDoctorProfileCommand request, CancellationToken ct)
    {
        if (currentUser.UserId is null || currentTenant.TenantId is null)
        {
            return Result.Failure<DoctorDto>(Unauthorized);
        }

        var hasDocterRole = currentUser.Roles.Contains("doctor", StringComparer.OrdinalIgnoreCase);
        if (!hasDocterRole)
        {
            return Result.Failure<DoctorDto>(Unauthorized);
        }

        var tenantId = currentTenant.TenantId.Value;
        var userId = currentUser.UserId.Value;

        var crmExists = await doctorRepository.ExistsByCrmAsync(tenantId, request.Crm, request.CouncilState, ct);
        if (crmExists)
        {
            return Result.Failure<DoctorDto>(CrmAlreadyExists);
        }

        var existingProfile = await doctorRepository.GetByCurrentUserAsync(userId, ct);
        if (existingProfile is not null)
        {
            return Result.Failure<DoctorDto>(ProfileAlreadyExists);
        }

        var doctorResult = DoctorProfile.Create(tenantId, request.Name, request.Crm, request.CouncilState, request.Specialty);
        if (doctorResult.IsFailure)
        {
            return Result.Failure<DoctorDto>(doctorResult.Error);
        }

        var doctor = doctorResult.Value;
        var linkResult = doctor.LinkUser(userId);
        if (linkResult.IsFailure)
        {
            return Result.Failure<DoctorDto>(linkResult.Error);
        }

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
