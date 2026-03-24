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
        var primaryTenant = tenants.Count > 0 ? tenants[0] : null;
        Guid? tenantId = null;
        var roles = new List<string>();

        if (primaryTenant is not null)
        {
            tenantId = primaryTenant.Id;
            TenantMember? member = null;
            foreach (var m in primaryTenant.Members)
            {
                if (m.UserId == user.Id) { member = m; break; }
            }

            if (member is not null)
            {
                roles.Add(member.Role.ToString().ToLowerInvariant());
            }
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
