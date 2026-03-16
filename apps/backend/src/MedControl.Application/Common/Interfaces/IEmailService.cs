namespace MedControl.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendMagicLinkAsync(string toEmail, string magicLink, CancellationToken ct = default);
}
