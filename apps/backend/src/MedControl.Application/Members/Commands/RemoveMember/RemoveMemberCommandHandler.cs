using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;

namespace MedControl.Application.Members.Commands.RemoveMember;

public sealed class RemoveMemberCommandHandler(
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenant,
    ICurrentUserService currentUser)
    : IRequestHandler<RemoveMemberCommand, Result>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Member.Unauthorized", "A tenant context is required or insufficient permissions.");

    private static readonly Error TenantNotFound =
        Error.NotFound("Member.TenantNotFound", "Tenant not found.");

    public async Task<Result> Handle(RemoveMemberCommand request, CancellationToken ct)
    {
        if (currentTenant.TenantId is null)
        {
            return Result.Failure(Unauthorized);
        }

        var hasPermission = currentUser.Roles.Contains("admin", StringComparer.OrdinalIgnoreCase)
            || currentUser.Roles.Contains("owner", StringComparer.OrdinalIgnoreCase);

        if (!hasPermission)
        {
            return Result.Failure(Unauthorized);
        }

        var tenantId = currentTenant.TenantId.Value;

        var tenant = await tenantRepository.GetByIdAsync(tenantId, ct);
        if (tenant is null)
        {
            return Result.Failure(TenantNotFound);
        }

        var removeResult = tenant.RemoveMember(request.UserId);
        if (removeResult.IsFailure)
        {
            return removeResult;
        }

        await tenantRepository.UpdateAsync(tenant, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
