namespace MedControl.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendMagicLinkAsync(string toEmail, string magicLink, CancellationToken ct = default);
    Task SendInvitationAsync(string toEmail, string inviteLink, CancellationToken ct = default);
}
