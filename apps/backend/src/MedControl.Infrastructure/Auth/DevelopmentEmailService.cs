using MedControl.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace MedControl.Infrastructure.Auth;

internal sealed class DevelopmentEmailService(ILogger<DevelopmentEmailService> logger) : IEmailService
{
    public Task SendMagicLinkAsync(string email, string magicLink, CancellationToken ct = default)
    {
        logger.LogWarning(
            """

            ==========================================
            [DEV] Magic Link para {Email}
            {MagicLink}
            ==========================================
            """,
            email,
            magicLink);

        return Task.CompletedTask;
    }
}
