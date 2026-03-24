using MedControl.Application.Auth.DTOs;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;
using MedControl.Domain.Users;

namespace MedControl.Application.Auth.Commands.GoogleVerifyIdToken;

public sealed class GoogleVerifyIdTokenCommandHandler(
    IGoogleAuthService googleAuthService,
    IUserRepository userRepository,
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork,
    ITokenService tokenService)
    : IRequestHandler<GoogleVerifyIdTokenCommand, Result<AuthTokenDto>>
{
    private static readonly Error GoogleAuthFailed =
        Error.Unauthorized("Auth.GoogleAuthFailed", "Google authentication failed.");

    private static readonly Error NoTenantAccess =
        Error.Unauthorized("Auth.NoTenantAccess", "Your account is not associated with any tenant. Contact your administrator.");

    public async Task<Result<AuthTokenDto>> Handle(GoogleVerifyIdTokenCommand request, CancellationToken ct)
    {
        var googleUserInfo = await googleAuthService.VerifyIdTokenAsync(request.IdToken, ct);
        if (googleUserInfo is null)
        {
            return Result.Failure<AuthTokenDto>(GoogleAuthFailed);
        }

        var user = await userRepository.GetByEmailAsync(googleUserInfo.Email, ct);
        if (user is null)
        {
            user = User.CreateFromGoogle(googleUserInfo.Email, googleUserInfo.DisplayName, googleUserInfo.AvatarUrl).Value;
            user.RecordLogin();
            await userRepository.AddAsync(user, ct);
        }
        else
        {
            user.RecordLogin();
            await userRepository.UpdateAsync(user, ct);
        }

        await unitOfWork.SaveChangesAsync(ct);

        var globalRoles = new List<string>();
        if (user.IsGlobalAdmin())
        {
            globalRoles.Add("admin");
        }
        else if (user.IsGlobalSupport())
        {
            globalRoles.Add("support");
        }

        var tenants = await tenantRepository.ListByUserAsync(user.Id, ct);
        if (tenants.Count == 0)
        {
            return Result.Failure<AuthTokenDto>(NoTenantAccess);
        }

        var primaryTenant = tenants[0];
        var tenantId = (Guid?)primaryTenant.Id;
        var roles = new List<string>();

        TenantMember? member = null;
        foreach (var m in primaryTenant.Members)
        {
            if (m.UserId == user.Id) { member = m; break; }
        }

        if (member is not null)
        {
            roles.Add(member.Role.ToString().ToLowerInvariant());
        }

        var tokenPair = tokenService.GenerateTokenPair(
            user.Id,
            user.Email,
            tenantId,
            roles,
            globalRoles);

        return Result.Success(new AuthTokenDto(
            tokenPair.AccessToken,
            tokenPair.RefreshToken,
            tokenPair.ExpiresAt));
    }
}
