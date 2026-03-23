using System.Net.Http.Json;
using System.Text.Json.Serialization;
using MedControl.Application.Common.Interfaces;
using MedControl.Infrastructure.Auth.Settings;
using Microsoft.Extensions.Options;

namespace MedControl.Infrastructure.Auth;

internal sealed class GoogleAuthService(
    HttpClient httpClient,
    IOptions<GoogleAuthSettings> settings)
    : IGoogleAuthService
{
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
    private const string UserInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";
    private const string TokenInfoEndpoint = "https://oauth2.googleapis.com/tokeninfo";

    public async Task<GoogleUserInfo?> ExchangeCodeAsync(string code, string redirectUri, CancellationToken ct = default)
    {
        var tokenResponse = await ExchangeCodeForTokenAsync(code, redirectUri, ct);
        if (tokenResponse?.AccessToken is null)
        {
            return null;
        }

        return await GetUserInfoAsync(tokenResponse.AccessToken, ct);
    }

    private async Task<GoogleTokenResponse?> ExchangeCodeForTokenAsync(string code, string redirectUri, CancellationToken ct)
    {
        var parameters = new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = settings.Value.ClientId,
            ["client_secret"] = settings.Value.ClientSecret,
            ["redirect_uri"] = redirectUri,
            ["grant_type"] = "authorization_code",
        };

        var response = await httpClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(parameters), ct);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<GoogleTokenResponse>(ct);
    }

    private async Task<GoogleUserInfo?> GetUserInfoAsync(string accessToken, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, UserInfoEndpoint);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var userInfo = await response.Content.ReadFromJsonAsync<GoogleUserInfoResponse>(ct);
        if (userInfo?.Email is null)
        {
            return null;
        }

        Uri? avatarUrl = Uri.TryCreate(userInfo.Picture, UriKind.Absolute, out var uri) ? uri : null;

        return new GoogleUserInfo(userInfo.Email, userInfo.Name ?? userInfo.Email, avatarUrl);
    }

    public async Task<GoogleUserInfo?> VerifyIdTokenAsync(string idToken, CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync(
            $"{TokenInfoEndpoint}?id_token={Uri.EscapeDataString(idToken)}", ct);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var tokenInfo = await response.Content.ReadFromJsonAsync<GoogleTokenInfoResponse>(ct);
        if (tokenInfo?.Email is null)
        {
            return null;
        }

        Uri? avatarUrl = Uri.TryCreate(tokenInfo.Picture, UriKind.Absolute, out var uri) ? uri : null;
        return new GoogleUserInfo(tokenInfo.Email, tokenInfo.Name ?? tokenInfo.Email, avatarUrl);
    }

    private sealed record GoogleTokenResponse([property: JsonPropertyName("access_token")] string? AccessToken);

    private sealed record GoogleUserInfoResponse(
        [property: JsonPropertyName("email")] string? Email,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("picture")] string? Picture);

    private sealed record GoogleTokenInfoResponse(
        [property: JsonPropertyName("email")] string? Email,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("picture")] string? Picture);
}
