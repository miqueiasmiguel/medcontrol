using MedControl.Application.Members.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Application.Members.Commands.AddMember;

public record AddMemberCommand(string Email, string Role) : ICommand<Result<MemberDto>>;
