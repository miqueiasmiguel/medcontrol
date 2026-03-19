using MedControl.Application.Mediator;
using MedControl.Application.Users.DTOs;
using MedControl.Domain.Common;

namespace MedControl.Application.Users.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IQuery<Result<UserDto>>;
