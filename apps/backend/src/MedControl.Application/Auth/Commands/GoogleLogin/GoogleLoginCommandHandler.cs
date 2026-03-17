using MedControl.Application.Auth.DTOs;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;
using MedControl.Domain.Users;

namespace MedControl.Application.Auth.Commands.GoogleLogin;

public sealed class GoogleLoginCommandHandler(
    IGoogleAuthService googleAuthService,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ITokenService tokenService)
    : IRequestHandler<GoogleLoginCommand, Result<AuthTokenDto>>
{
    private static readonly Error GoogleAuthFailed =
        Error.Unauthorized("Auth.GoogleAuthFailed", "Google authentication failed.");

    public async Task<Result<AuthTokenDto>> Handle(GoogleLoginCommand request, CancellationToken ct)
    {
        var googleUserInfo = await googleAuthService.ExchangeCodeAsync(request.Code, request.RedirectUri, ct);
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

        var tokenPair = tokenService.GenerateTokenPair(
            user.Id,
            user.Email,
            tenantId: null,
            roles: [],
            globalRoles: globalRoles);

        return Result.Success(new AuthTokenDto(
            tokenPair.AccessToken,
            tokenPair.RefreshToken,
            tokenPair.ExpiresAt));
    }
}
