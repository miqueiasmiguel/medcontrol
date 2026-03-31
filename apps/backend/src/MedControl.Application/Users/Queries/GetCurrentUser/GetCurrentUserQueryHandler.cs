using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using MedControl.Application.Users.DTOs;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;
using MedControl.Domain.Users;

namespace MedControl.Application.Users.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(
    ICurrentUserService currentUser,
    IUserRepository userRepository,
    ITenantRepository tenantRepository)
    : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("User.Unauthorized", "User is not authenticated.");

    private static readonly Error NotFound =
        Error.NotFound("User.NotFound", "User not found.");

    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken ct)
    {
        if (currentUser.UserId is null)
        {
            return Result.Failure<UserDto>(Unauthorized);
        }

        var user = await userRepository.GetByIdAsync(currentUser.UserId.Value, ct);
        if (user is null)
        {
            return Result.Failure<UserDto>(NotFound);
        }

        var tenantRole = currentUser.Roles.Count > 0 ? currentUser.Roles[0] : null;

        string? tenantName = null;
        if (currentUser.TenantId.HasValue)
        {
            var tenant = await tenantRepository.GetByIdAsync(currentUser.TenantId.Value, ct);
            tenantName = tenant?.Name;
        }

        return Result.Success(ToDto(user, tenantRole, tenantName));
    }

    internal static UserDto ToDto(User user, string? tenantRole = null, string? tenantName = null) => new(
        user.Id,
        user.Email,
        user.DisplayName,
        user.AvatarUrl?.ToString(),
        user.IsEmailVerified,
        user.GlobalRole.ToString(),
        user.LastLoginAt,
        tenantRole,
        tenantName);
}
