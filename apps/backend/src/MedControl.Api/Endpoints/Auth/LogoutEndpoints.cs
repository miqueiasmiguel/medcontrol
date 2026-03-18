namespace MedControl.Api.Endpoints.Auth;

public static class LogoutEndpoints
{
    public static RouteGroupBuilder MapLogout(this RouteGroupBuilder group)
    {
        group.MapPost("/logout", Logout)
             .WithName("Logout")
             .Produces(StatusCodes.Status204NoContent);

        return group;
    }

    private static IResult Logout(HttpContext ctx)
    {
        CookieHelper.ClearAuthCookies(ctx);
        return Results.NoContent();
    }
}
