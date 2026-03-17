using MedControl.Application.Auth.DTOs;

namespace MedControl.Api.Endpoints.Auth;

public static class CookieHelper
{
    public static void SetAuthCookies(HttpContext ctx, AuthTokenDto dto)
    {
        var httpOnly = new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Secure = ctx.Request.IsHttps,
            Path = "/",
        };

        var session = new CookieOptions
        {
            HttpOnly = false,
            SameSite = SameSiteMode.Strict,
            Secure = ctx.Request.IsHttps,
            Path = "/",
        };

        ctx.Response.Cookies.Append("mmc_access_token", dto.AccessToken, httpOnly);
        ctx.Response.Cookies.Append("mmc_refresh_token", dto.RefreshToken, httpOnly);
        ctx.Response.Cookies.Append("mmc_session", "1", session);
    }

    public static void ClearAuthCookies(HttpContext ctx)
    {
        var expired = new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Secure = ctx.Request.IsHttps,
            Path = "/",
            MaxAge = TimeSpan.Zero,
        };

        var expiredSession = new CookieOptions
        {
            HttpOnly = false,
            SameSite = SameSiteMode.Strict,
            Secure = ctx.Request.IsHttps,
            Path = "/",
            MaxAge = TimeSpan.Zero,
        };

        ctx.Response.Cookies.Append("mmc_access_token", "", expired);
        ctx.Response.Cookies.Append("mmc_refresh_token", "", expired);
        ctx.Response.Cookies.Append("mmc_session", "", expiredSession);
    }
}
