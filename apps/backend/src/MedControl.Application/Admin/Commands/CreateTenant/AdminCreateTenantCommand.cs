using MedControl.Application.Admin.DTOs;
using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Application.Admin.Commands.CreateTenant;

public record AdminCreateTenantCommand(string Name, string OwnerEmail) : ICommand<Result<AdminTenantDto>>;
