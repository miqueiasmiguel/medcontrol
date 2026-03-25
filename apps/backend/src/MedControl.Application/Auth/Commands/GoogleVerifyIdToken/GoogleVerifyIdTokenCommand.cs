using MedControl.Application.Auth.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Application.Auth.Commands.GoogleVerifyIdToken;

public record GoogleVerifyIdTokenCommand(string IdToken) : ICommand<Result<AuthTokenDto>>;
