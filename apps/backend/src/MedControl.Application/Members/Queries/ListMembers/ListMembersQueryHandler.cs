using MedControl.Application.Common.Interfaces;
using MedControl.Application.Members.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;
using MedControl.Domain.Users;

namespace MedControl.Application.Members.Queries.ListMembers;

public sealed class ListMembersQueryHandler(
    ITenantRepository tenantRepository,
    IUserRepository userRepository,
    ICurrentTenantService currentTenant)
    : IRequestHandler<ListMembersQuery, Result<IReadOnlyList<MemberDto>>>
{
    private static readonly Error Unauthorized =
        Error.Unauthorized("Member.Unauthorized", "A tenant context is required.");

    private static readonly Error TenantNotFound =
        Error.NotFound("Member.TenantNotFound", "Tenant not found.");

    public async Task<Result<IReadOnlyList<MemberDto>>> Handle(ListMembersQuery request, CancellationToken ct)
    {
        if (currentTenant.TenantId is null)
        {
            return Result.Failure<IReadOnlyList<MemberDto>>(Unauthorized);
        }

        var tenantId = currentTenant.TenantId.Value;

        var tenant = await tenantRepository.GetByIdAsync(tenantId, ct);
        if (tenant is null)
        {
            return Result.Failure<IReadOnlyList<MemberDto>>(TenantNotFound);
        }

        var memberUserIds = tenant.Members.Select(m => m.UserId).ToList();
        var users = await userRepository.GetByIdsAsync(memberUserIds, ct);
        var userDict = users.ToDictionary(u => u.Id);

        var dtos = tenant.Members
            .Select(m =>
            {
                userDict.TryGetValue(m.UserId, out var user);
                return new MemberDto(
                    m.UserId,
                    user?.DisplayName,
                    user?.Email,
                    user?.AvatarUrl?.ToString(),
                    m.Role.ToString().ToLowerInvariant(),
                    m.JoinedAt);
            })
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<MemberDto>>(dtos);
    }
}
