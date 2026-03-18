using MedControl.Application.Auth.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Application.Tenants.Commands.SwitchTenant;

public record SwitchTenantCommand(Guid TenantId) : ICommand<Result<AuthTokenDto>>;
