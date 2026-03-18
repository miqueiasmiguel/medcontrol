using MedControl.Application.Auth.DTOs;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;

namespace MedControl.Application.Tenants.Commands.CreateTenant;

public sealed class CreateTenantCommandHandler(
    ICurrentUserService currentUser,
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork,
    ITokenService tokenService)
    : IRequestHandler<CreateTenantCommand, Result<AuthTokenDto>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Tenant.Unauthorized", "User is not authenticated.");

    public async Task<Result<AuthTokenDto>> Handle(CreateTenantCommand request, CancellationToken ct)
    {
        if (currentUser.UserId is null || currentUser.Email is null)
        {
            return Result.Failure<AuthTokenDto>(Unauthorized);
        }

        var tenantResult = Tenant.Create(request.Name);
        if (tenantResult.IsFailure)
        {
            return Result.Failure<AuthTokenDto>(tenantResult.Error);
        }

        var tenant = tenantResult.Value;
        var memberResult = tenant.AddMember(currentUser.UserId.Value, TenantRole.Owner);
        if (memberResult.IsFailure)
        {
            return Result.Failure<AuthTokenDto>(memberResult.Error);
        }

        await tenantRepository.AddAsync(tenant, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var tokenPair = tokenService.GenerateTokenPair(
            currentUser.UserId.Value,
            currentUser.Email,
            tenant.Id,
            roles: ["owner"],
            globalRoles: currentUser.GlobalRoles);

        return Result.Success(new AuthTokenDto(
            tokenPair.AccessToken,
            tokenPair.RefreshToken,
            tokenPair.ExpiresAt));
    }
}
