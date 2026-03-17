using MedControl.Application.Auth.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Application.Auth.Commands.GoogleLogin;

public record GoogleLoginCommand(string Code, string RedirectUri) : ICommand<Result<AuthTokenDto>>;
