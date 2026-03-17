using MedControl.Api.Endpoints.Auth;

namespace MedControl.Api.Endpoints;

public static class EndpointExtensions
{
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        var auth = app.MapGroup("auth");
        auth.MapGroup("magic-link").MapMagicLink();

        return app;
    }
}
