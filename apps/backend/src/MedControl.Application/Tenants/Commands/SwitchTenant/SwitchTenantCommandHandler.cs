using MedControl.Application.Auth.DTOs;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;

namespace MedControl.Application.Tenants.Commands.SwitchTenant;

public sealed class SwitchTenantCommandHandler(
    ICurrentUserService currentUser,
    ITenantRepository tenantRepository,
    ITokenService tokenService)
    : IRequestHandler<SwitchTenantCommand, Result<AuthTokenDto>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Tenant.Unauthorized", "User is not authenticated.");

    private static readonly Error NotFound =
        Error.NotFound("Tenant.NotFound", "Tenant not found or user is not a member.");

    private static readonly Error TenantDisabled =
        Error.Unauthorized("Auth.TenantDisabled", "Your tenant has been disabled. Contact support.");

    public async Task<Result<AuthTokenDto>> Handle(SwitchTenantCommand request, CancellationToken ct)
    {
        if (currentUser.UserId is null || currentUser.Email is null)
        {
            return Result.Failure<AuthTokenDto>(Unauthorized);
        }

        var userId = currentUser.UserId.Value;
        var tenants = await tenantRepository.ListByUserAsync(userId, ct);
        var tenant = tenants.FirstOrDefault(t => t.Id == request.TenantId);

        if (tenant is null)
        {
            return Result.Failure<AuthTokenDto>(NotFound);
        }

        if (!tenant.IsActive)
        {
            return Result.Failure<AuthTokenDto>(TenantDisabled);
        }

        var member = tenant.Members.FirstOrDefault(m => m.UserId == userId);
        if (member is null)
        {
            return Result.Failure<AuthTokenDto>(NotFound);
        }

        var role = member.Role.ToString().ToLowerInvariant();
        var tokenPair = tokenService.GenerateTokenPair(
            userId,
            currentUser.Email,
            tenant.Id,
            roles: [role],
            globalRoles: currentUser.GlobalRoles);

        return Result.Success(new AuthTokenDto(
            tokenPair.AccessToken,
            tokenPair.RefreshToken,
            tokenPair.ExpiresAt));
    }
}
