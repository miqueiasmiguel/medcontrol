using MedControl.Application.Common.Interfaces;
using Resend;

namespace MedControl.Infrastructure.Auth;

internal sealed class EmailService(IResend resend) : IEmailService
{
    public async Task SendMagicLinkAsync(string email, string magicLink, CancellationToken ct = default)
    {
        var message = new EmailMessage();
        message.From = "noreply@medcontrol.app";
        message.To.Add(email);
        message.Subject = "Your MedControl sign-in link";
        message.HtmlBody = $"""
            <p>Click the link below to sign in to MedControl:</p>
            <p><a href="{magicLink}">Sign in to MedControl</a></p>
            <p>This link expires in 15 minutes and can only be used once.</p>
            """;

        await resend.EmailSendAsync(message, ct);
    }
}
