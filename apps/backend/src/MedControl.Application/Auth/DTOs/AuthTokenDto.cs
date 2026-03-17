namespace MedControl.Application.Auth.DTOs;

public record AuthTokenDto(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);
