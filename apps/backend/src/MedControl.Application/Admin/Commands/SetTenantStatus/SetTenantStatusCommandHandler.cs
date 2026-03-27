using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;

namespace MedControl.Application.Admin.Commands.SetTenantStatus;

public sealed class SetTenantStatusCommandHandler(
    ICurrentUserService currentUser,
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SetTenantStatusCommand, Result>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Admin.Unauthorized", "Global admin role required.");

    private static readonly Error NotFound =
        Error.NotFound("Admin.TenantNotFound", "Tenant not found.");

    public async Task<Result> Handle(SetTenantStatusCommand request, CancellationToken ct)
    {
        if (!currentUser.HasGlobalRole("admin"))
        {
            return Result.Failure(Unauthorized);
        }

        var tenant = await tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
        {
            return Result.Failure(NotFound);
        }

        if (request.IsActive)
        {
            tenant.Activate();
        }
        else
        {
            tenant.Deactivate();
        }

        await tenantRepository.UpdateAsync(tenant, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
