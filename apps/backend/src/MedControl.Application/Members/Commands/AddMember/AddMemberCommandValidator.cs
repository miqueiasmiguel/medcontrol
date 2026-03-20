using FluentValidation;
using MedControl.Domain.Tenants;

namespace MedControl.Application.Members.Commands.AddMember;

public sealed class AddMemberCommandValidator : AbstractValidator<AddMemberCommand>
{
    public AddMemberCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(r => Enum.TryParse<TenantRole>(r, ignoreCase: true, out _))
            .WithMessage("Role must be a valid tenant role.");
    }
}
