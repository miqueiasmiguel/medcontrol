using MedControl.Application.Common.Interfaces;
using MedControl.Application.Members.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;
using MedControl.Domain.Users;

namespace MedControl.Application.Members.Commands.AddMember;

public sealed class AddMemberCommandHandler(
    ITenantRepository tenantRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenant,
    ICurrentUserService currentUser)
    : IRequestHandler<AddMemberCommand, Result<MemberDto>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Member.Unauthorized", "A tenant context is required or insufficient permissions.");

    private static readonly Error UserNotFound =
        Error.NotFound("Member.UserNotFound", "User with the specified email was not found.");

    private static readonly Error TenantNotFound =
        Error.NotFound("Member.TenantNotFound", "Tenant not found.");

    public async Task<Result<MemberDto>> Handle(AddMemberCommand request, CancellationToken ct)
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

        var user = await userRepository.GetByEmailAsync(request.Email, ct);
        if (user is null)
        {
            return Result.Failure<MemberDto>(UserNotFound);
        }

        var tenant = await tenantRepository.GetByIdAsync(tenantId, ct);
        if (tenant is null)
        {
            return Result.Failure<MemberDto>(TenantNotFound);
        }

        var role = Enum.Parse<TenantRole>(request.Role, ignoreCase: true);
        var addResult = tenant.AddMember(user.Id, role);
        if (addResult.IsFailure)
        {
            return Result.Failure<MemberDto>(addResult.Error);
        }

        await tenantRepository.UpdateAsync(tenant, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var member = tenant.Members.Single(m => m.UserId == user.Id);
        return Result.Success(new MemberDto(
            user.Id,
            user.DisplayName,
            user.Email,
            user.AvatarUrl?.ToString(),
            member.Role.ToString().ToLowerInvariant(),
            member.JoinedAt));
    }
}
