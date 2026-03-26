using MedControl.Application.Common.Interfaces;
using MedControl.Application.Doctors.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;
using MedControl.Domain.Doctors;
using MedControl.Domain.Tenants;

namespace MedControl.Application.Doctors.Commands.LinkDoctorProfileToUser;

public sealed class LinkDoctorProfileToUserCommandHandler(
    IDoctorRepository doctorRepository,
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenant,
    ICurrentUserService currentUser)
    : IRequestHandler<LinkDoctorProfileToUserCommand, Result<DoctorDto>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Doctor.Unauthorized", "A tenant context is required or insufficient permissions.");

    private static readonly Error DoctorNotFound =
        Error.NotFound("Doctor.NotFound", "Doctor profile not found.");

    private static readonly Error UserMustBeDoctorMember =
        Error.Validation("Doctor.UserMustBeDoctorMember", "The specified user must be a tenant member with the doctor role.");

    public async Task<Result<DoctorDto>> Handle(LinkDoctorProfileToUserCommand request, CancellationToken ct)
    {
        if (currentTenant.TenantId is null)
        {
            return Result.Failure<DoctorDto>(Unauthorized);
        }

        var hasPermission = currentUser.Roles.Contains("admin", StringComparer.OrdinalIgnoreCase)
            || currentUser.Roles.Contains("owner", StringComparer.OrdinalIgnoreCase);

        if (!hasPermission)
        {
            return Result.Failure<DoctorDto>(Unauthorized);
        }

        var tenantId = currentTenant.TenantId.Value;

        var doctor = await doctorRepository.GetByIdAsync(request.DoctorId, ct);
        if (doctor is null)
        {
            return Result.Failure<DoctorDto>(DoctorNotFound);
        }

        var tenant = await tenantRepository.GetByIdAsync(tenantId, ct);
        if (tenant is null)
        {
            return Result.Failure<DoctorDto>(Unauthorized);
        }

        var member = tenant.Members.SingleOrDefault(m => m.UserId == request.UserId);
        if (member is null || member.Role != TenantRole.Doctor)
        {
            return Result.Failure<DoctorDto>(UserMustBeDoctorMember);
        }

        var linkResult = doctor.LinkUser(request.UserId);
        if (linkResult.IsFailure)
        {
            return Result.Failure<DoctorDto>(linkResult.Error);
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
