using MedControl.Application.Mediator;
using MedControl.Application.Users.DTOs;
using MedControl.Domain.Common;

namespace MedControl.Application.Users.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(string? DisplayName) : ICommand<Result<UserDto>>;
