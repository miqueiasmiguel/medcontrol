using FluentValidation;
using MedControl.Domain.Tenants;

namespace MedControl.Application.Members.Commands.UpdateMemberRole;

public sealed class UpdateMemberRoleCommandValidator : AbstractValidator<UpdateMemberRoleCommand>
{
    public UpdateMemberRoleCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(r => Enum.TryParse<TenantRole>(r, ignoreCase: true, out _))
            .WithMessage("Role must be a valid tenant role.");
    }
}
