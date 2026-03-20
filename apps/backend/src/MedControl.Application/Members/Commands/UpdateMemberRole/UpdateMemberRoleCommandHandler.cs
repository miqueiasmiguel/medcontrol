using MedControl.Application.Common.Interfaces;
using MedControl.Application.Members.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;
using MedControl.Domain.Users;

namespace MedControl.Application.Members.Commands.UpdateMemberRole;

public sealed class UpdateMemberRoleCommandHandler(
    ITenantRepository tenantRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenant,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateMemberRoleCommand, Result<MemberDto>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Member.Unauthorized", "A tenant context is required or insufficient permissions.");

    private static readonly Error TenantNotFound =
        Error.NotFound("Member.TenantNotFound", "Tenant not found.");

    private static readonly Error UserNotFound =
        Error.NotFound("Member.UserNotFound", "User not found.");

    public async Task<Result<MemberDto>> Handle(UpdateMemberRoleCommand request, CancellationToken ct)
    {
        if (currentTenant.TenantId is null)
        {
            return Result.Failure<MemberDto>(Unauthorized);
        }

        var hasPermission = currentUser.Roles.Contains("admin", StringComparer.OrdinalIgnoreCase)
            || currentUser.Roles.Contains("owner", StringComparer.OrdinalIgnoreCase);

        if (!hasPermission)
        {
            return Result.Failure<MemberDto>(Unauthorized);
        }

        var tenantId = currentTenant.TenantId.Value;
        var currentUserId = currentUser.UserId ?? Guid.Empty;

        var tenant = await tenantRepository.GetByIdAsync(tenantId, ct);
        if (tenant is null)
        {
            return Result.Failure<MemberDto>(TenantNotFound);
        }

        var role = Enum.Parse<TenantRole>(request.Role, ignoreCase: true);
        var updateResult = tenant.UpdateMemberRole(request.UserId, currentUserId, role);
        if (updateResult.IsFailure)
        {
            return Result.Failure<MemberDto>(updateResult.Error);
        }

        await tenantRepository.UpdateAsync(tenant, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var user = await userRepository.GetByIdAsync(request.UserId, ct);
        var member = tenant.Members.Single(m => m.UserId == request.UserId);

        return Result.Success(new MemberDto(
            request.UserId,
            user?.DisplayName,
            user?.Email,
            user?.AvatarUrl?.ToString(),
            member.Role.ToString().ToLowerInvariant(),
            member.JoinedAt));
    }
}
