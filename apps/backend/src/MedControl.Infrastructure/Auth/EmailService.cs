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

    public async Task SendInvitationAsync(string toEmail, string inviteLink, CancellationToken ct = default)
    {
        var message = new EmailMessage();
        message.From = "noreply@medcontrol.app";
        message.To.Add(toEmail);
        message.Subject = "Você foi convidado para o MedControl";
        message.HtmlBody = $"""
            <p>Você foi convidado para acessar o MedControl.</p>
            <p>Clique no link abaixo para criar sua conta e entrar:</p>
            <p><a href="{inviteLink}">Acessar o MedControl</a></p>
            <p>Este link expira em 15 minutos e pode ser usado apenas uma vez.</p>
            """;

        await resend.EmailSendAsync(message, ct);
    }
}
