using MedControl.Application.Members.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Application.Members.Commands.UpdateMemberRole;

public record UpdateMemberRoleCommand(Guid UserId, string Role) : ICommand<Result<MemberDto>>;
