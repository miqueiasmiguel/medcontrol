using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Application.Members.Commands.RemoveMember;

public record RemoveMemberCommand(Guid UserId) : ICommand<Result>;
