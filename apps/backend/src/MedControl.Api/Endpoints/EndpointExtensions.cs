using MedControl.Api.Endpoints.Auth;
using MedControl.Api.Endpoints.Doctors;
using MedControl.Api.Endpoints.Tenants;

namespace MedControl.Api.Endpoints;

public static class EndpointExtensions
{
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        var auth = app.MapGroup("auth");
        auth.MapGroup("magic-link").MapMagicLink();
        auth.MapGroup("google").MapGoogleAuth();
        auth.MapLogout();

        var tenants = app.MapGroup("tenants");
        tenants.MapTenants();

        var doctors = app.MapGroup("doctors");
        doctors.MapDoctors();

        return app;
    }
}
