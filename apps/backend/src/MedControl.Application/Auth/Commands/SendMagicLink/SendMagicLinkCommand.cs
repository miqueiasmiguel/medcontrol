using MedControl.Application.Mediator;

namespace MedControl.Application.Auth.Commands.SendMagicLink;

public record SendMagicLinkCommand(string Email) : ICommand;
