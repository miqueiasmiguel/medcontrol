using MedControl.Application.Mediator;
using MedControl.Domain.Common;

namespace MedControl.Application.Admin.Commands.SetTenantStatus;

public record SetTenantStatusCommand(Guid TenantId, bool IsActive) : ICommand<Result>;
