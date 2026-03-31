using MedControl.Application.Admin.DTOs;
using MedControl.Application.Auth.Settings;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;
using MedControl.Domain.Users;
using Microsoft.Extensions.Options;

namespace MedControl.Application.Admin.Commands.CreateTenant;

public sealed class AdminCreateTenantCommandHandler(
    ITenantRepository tenantRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IEmailService emailService,
    IMagicLinkService magicLinkService,
    IOptions<MagicLinkSettings> magicLinkSettings)
    : IRequestHandler<AdminCreateTenantCommand, Result<AdminTenantDto>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Admin.Unauthorized", "Global admin role required.");

    public async Task<Result<AdminTenantDto>> Handle(AdminCreateTenantCommand request, CancellationToken ct)
    {
        if (!currentUser.HasGlobalRole("admin"))
        {
            return Result.Failure<AdminTenantDto>(Unauthorized);
        }

        var tenantResult = Tenant.Create(request.Name);
        if (tenantResult.IsFailure)
        {
            return Result.Failure<AdminTenantDto>(tenantResult.Error);
        }

        var tenant = tenantResult.Value;

        var user = await userRepository.GetByEmailAsync(request.OwnerEmail, ct);
        var sendInvite = false;

        if (user is null)
        {
            user = User.Create(request.OwnerEmail).Value;
            await userRepository.AddAsync(user, ct);
            sendInvite = true;
        }

        tenant.AddMember(user.Id, TenantRole.Owner);
        await tenantRepository.AddAsync(tenant, ct);
        await unitOfWork.SaveChangesAsync(ct);

        if (sendInvite)
        {
            var token = await magicLinkService.GenerateTokenAsync(user.Email, ct);
            var url = $"{magicLinkSettings.Value.BaseUrl}?token={token}";
            await emailService.SendInvitationAsync(user.Email, url, ct);
        }

        return Result.Success(new AdminTenantDto(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.IsActive,
            tenant.CreatedAt,
            tenant.Members.Count));
    }
}
