using MedControl.Application.Auth.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Application.Auth.Commands.VerifyMagicLink;

public record VerifyMagicLinkCommand(string Token) : ICommand<Result<AuthTokenDto>>;
