using FluentValidation;

namespace MedControl.Application.Admin.Commands.CreateTenant;

public sealed class AdminCreateTenantCommandValidator : AbstractValidator<AdminCreateTenantCommand>
{
    public AdminCreateTenantCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OwnerEmail).NotEmpty().EmailAddress();
    }
}
