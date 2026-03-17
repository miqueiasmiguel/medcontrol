using MedControl.Application.Auth.Settings;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Mediator;
using MedControl.Domain.Users;
using Microsoft.Extensions.Options;

namespace MedControl.Application.Auth.Commands.SendMagicLink;

public sealed class SendMagicLinkCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IMagicLinkService magicLinkService,
    IEmailService emailService,
    IOptions<MagicLinkSettings> magicLinkSettings)
    : IRequestHandler<SendMagicLinkCommand, Unit>
{
    public async Task<Unit> Handle(SendMagicLinkCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await userRepository.GetByEmailAsync(email, ct);
        if (user is null)
        {
            var result = User.Create(email);
            await userRepository.AddAsync(result.Value, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }

        var token = await magicLinkService.GenerateTokenAsync(email, ct);
        var url = $"{magicLinkSettings.Value.BaseUrl}?token={token}";
        await emailService.SendMagicLinkAsync(email, url, ct);

        return Unit.Value;
    }
}
