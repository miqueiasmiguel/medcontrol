using MedControl.Application.Auth.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Application.Tenants.Commands.CreateTenant;

public record CreateTenantCommand(string Name) : ICommand<Result<AuthTokenDto>>;
