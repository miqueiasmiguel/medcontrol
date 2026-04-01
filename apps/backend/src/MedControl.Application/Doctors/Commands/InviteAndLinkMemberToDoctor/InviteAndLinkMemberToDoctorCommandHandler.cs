using MedControl.Application.Auth.Settings;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Doctors.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;
using MedControl.Domain.Doctors;
using MedControl.Domain.Tenants;
using MedControl.Domain.Users;
using Microsoft.Extensions.Options;

namespace MedControl.Application.Doctors.Commands.InviteAndLinkMemberToDoctor;

public sealed class InviteAndLinkMemberToDoctorCommandHandler(
    IDoctorRepository doctorRepository,
    ITenantRepository tenantRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenant,
    ICurrentUserService currentUser,
    IEmailService emailService,
    IMagicLinkService magicLinkService,
    IOptions<MagicLinkSettings> magicLinkSettings)
    : IRequestHandler<InviteAndLinkMemberToDoctorCommand, Result<DoctorDto>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Doctor.Unauthorized", "A tenant context is required or insufficient permissions.");

    private static readonly Error DoctorNotFound =
        Error.NotFound("Doctor.NotFound", "Doctor profile not found.");

    private static readonly Error TenantNotFound =
        Error.NotFound("Doctor.TenantNotFound", "Tenant not found.");

    public async Task<Result<DoctorDto>> Handle(InviteAndLinkMemberToDoctorCommand request, CancellationToken ct)
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

        if (doctor.UserId.HasValue)
        {
            return Result.Failure<DoctorDto>(DoctorProfile.Errors.UserAlreadyLinked);
        }

        var user = await userRepository.GetByEmailAsync(request.Email, ct);
        var invited = false;

        if (user is null)
        {
            user = User.Create(request.Email).Value;
            await userRepository.AddAsync(user, ct);
            invited = true;
        }

        var tenant = await tenantRepository.GetByIdAsync(tenantId, ct);
        if (tenant is null)
        {
            return Result.Failure<DoctorDto>(TenantNotFound);
        }

        var existingMember = tenant.Members.SingleOrDefault(m => m.UserId == user.Id);
        if (existingMember is null)
        {
            var addResult = tenant.AddMember(user.Id, TenantRole.Doctor);
            if (addResult.IsFailure)
            {
                return Result.Failure<DoctorDto>(addResult.Error);
            }

            await tenantRepository.UpdateAsync(tenant, ct);
        }

        var linkResult = doctor.LinkUser(user.Id);
        if (linkResult.IsFailure)
        {
            return Result.Failure<DoctorDto>(linkResult.Error);
        }

        await doctorRepository.UpdateAsync(doctor, ct);
        await unitOfWork.SaveChangesAsync(ct);

        if (invited)
        {
            var token = await magicLinkService.GenerateInviteTokenAsync(request.Email, ct);
            var url = $"{magicLinkSettings.Value.BaseUrl}?token={token}";
            await emailService.SendInvitationAsync(request.Email, url, ct);
        }

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
