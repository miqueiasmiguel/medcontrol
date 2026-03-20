using MedControl.Application.Members.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Application.Members.Queries.ListMembers;

public record ListMembersQuery : IQuery<Result<IReadOnlyList<MemberDto>>>;
