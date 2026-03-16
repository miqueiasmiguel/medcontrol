namespace MedControl.Application.Common.Interfaces;

public interface IMagicLinkService
{
    Task<string> GenerateTokenAsync(string email, CancellationToken ct = default);
    Task<string?> ValidateTokenAsync(string token, CancellationToken ct = default);
}
