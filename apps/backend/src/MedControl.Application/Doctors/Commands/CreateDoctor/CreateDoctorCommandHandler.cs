using MedControl.Application.Auth.Settings;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Doctors.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;
using MedControl.Domain.Doctors;
using MedControl.Domain.Tenants;
using MedControl.Domain.Users;
using Microsoft.Extensions.Options;

namespace MedControl.Application.Doctors.Commands.CreateDoctor;

public sealed class CreateDoctorCommandHandler(
    IDoctorRepository doctorRepository,
    ITenantRepository tenantRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenant,
    ICurrentUserService currentUser,
    IEmailService emailService,
    IMagicLinkService magicLinkService,
    IOptions<MagicLinkSettings> magicLinkSettings)
    : IRequestHandler<CreateDoctorCommand, Result<DoctorDto>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Doctor.Unauthorized", "A tenant context is required.");

    private static readonly Error InsufficientPermissions =
        Error.Unauthorized("Doctor.InsufficientPermissions", "Only admins or owners can invite members.");

    private static readonly Error CrmAlreadyExists =
        Error.Conflict("Doctor.CrmAlreadyExists", "A doctor with this CRM and council state already exists in this tenant.");

    private static readonly Error TenantNotFound =
        Error.NotFound("Doctor.TenantNotFound", "Tenant not found.");

    public async Task<Result<DoctorDto>> Handle(CreateDoctorCommand request, CancellationToken ct)
    {
        if (currentTenant.TenantId is null)
        {
            return Result.Failure<DoctorDto>(Unauthorized);
        }

        var tenantId = currentTenant.TenantId.Value;

        if (request.InviteEmail is not null)
        {
            var hasPermission = currentUser.Roles.Contains("admin", StringComparer.OrdinalIgnoreCase)
                || currentUser.Roles.Contains("owner", StringComparer.OrdinalIgnoreCase);
            if (!hasPermission)
            {
                return Result.Failure<DoctorDto>(InsufficientPermissions);
            }
        }

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

        var invited = false;

        if (request.InviteEmail is not null)
        {
            var user = await userRepository.GetByEmailAsync(request.InviteEmail, ct);
            if (user is null)
            {
                user = User.Create(request.InviteEmail).Value;
                await userRepository.AddAsync(user, ct);
                invited = true;
            }

            var tenant = await tenantRepository.GetByIdAsync(tenantId, ct);
            if (tenant is null)
            {
                return Result.Failure<DoctorDto>(TenantNotFound);
            }

            var addMemberResult = tenant.AddMember(user.Id, Domain.Tenants.TenantRole.Doctor);
            if (addMemberResult.IsFailure)
            {
                return Result.Failure<DoctorDto>(addMemberResult.Error);
            }

            await tenantRepository.UpdateAsync(tenant, ct);

            var linkResult = doctor.LinkUser(user.Id);
            if (linkResult.IsFailure)
            {
                return Result.Failure<DoctorDto>(linkResult.Error);
            }

            // Do NOT call UpdateAsync here — the entity is already tracked as Added by AddAsync above.
            // Calling UpdateAsync would change the EF Core state to Modified, causing a
            // DbUpdateConcurrencyException when SaveChangesAsync tries to UPDATE a row that doesn't exist yet.
            // The UserId mutation via LinkUser is included automatically in the INSERT.
        }

        await unitOfWork.SaveChangesAsync(ct);

        if (invited)
        {
            var token = await magicLinkService.GenerateTokenAsync(request.InviteEmail!, ct);
            var url = $"{magicLinkSettings.Value.BaseUrl}?token={token}";
            await emailService.SendInvitationAsync(request.InviteEmail!, url, ct);
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
