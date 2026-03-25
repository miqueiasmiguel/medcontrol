using MedControl.Application.Auth.DTOs;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;
using MedControl.Domain.Users;

namespace MedControl.Application.Auth.Commands.VerifyMagicLink;

public sealed class VerifyMagicLinkCommandHandler(
    IMagicLinkService magicLinkService,
    IUserRepository userRepository,
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork,
    ITokenService tokenService)
    : IRequestHandler<VerifyMagicLinkCommand, Result<AuthTokenDto>>
{
    private static readonly Error InvalidToken =
        Error.Unauthorized("Auth.InvalidToken", "The magic link token is invalid or has expired.");

    private static readonly Error UserNotFound =
        Error.NotFound("Auth.UserNotFound", "No account was found for this email address.");

    public async Task<Result<AuthTokenDto>> Handle(VerifyMagicLinkCommand request, CancellationToken ct)
    {
        var email = await magicLinkService.ValidateTokenAsync(request.Token, ct);
        if (email is null)
        {
            return Result.Failure<AuthTokenDto>(InvalidToken);
        }

        var user = await userRepository.GetByEmailAsync(email, ct);
        if (user is null)
        {
            return Result.Failure<AuthTokenDto>(UserNotFound);
        }

        user.VerifyEmail();
        user.RecordLogin();

        var globalRoles = new List<string>();
        if (user.IsGlobalAdmin())
        {
            globalRoles.Add("admin");
        }
        else if (user.IsGlobalSupport())
        {
            globalRoles.Add("support");
        }

        await userRepository.UpdateAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var tenants = await tenantRepository.ListByUserAsync(user.Id, ct);

        Guid? tenantId = null;
        var roles = new List<string>();

        if (tenants.Count > 0)
        {
            var primaryTenant = tenants[0];
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
