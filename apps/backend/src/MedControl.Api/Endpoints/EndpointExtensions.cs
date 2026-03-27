using MedControl.Api.Endpoints.Admin;
using MedControl.Api.Endpoints.Auth;
using MedControl.Api.Endpoints.Doctors;
using MedControl.Api.Endpoints.HealthPlans;
using MedControl.Api.Endpoints.Members;
using MedControl.Api.Endpoints.Payments;
using MedControl.Api.Endpoints.Procedures;
using MedControl.Api.Endpoints.Tenants;
using MedControl.Api.Endpoints.Users;

namespace MedControl.Api.Endpoints;

public static class EndpointExtensions
{
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        var admin = app.MapGroup("admin");
        admin.MapGroup("tenants").MapAdminTenants();
        var auth = app.MapGroup("auth");
        auth.MapGroup("magic-link").MapMagicLink();
        auth.MapGroup("google").MapGoogleAuth();
        auth.MapLogout();

        var tenants = app.MapGroup("tenants");
        tenants.MapTenants();

        var doctors = app.MapGroup("doctors");
        doctors.MapDoctors();

        var healthPlans = app.MapGroup("health-plans");
        healthPlans.MapHealthPlans();

        var procedures = app.MapGroup("procedures");
        procedures.MapProcedures();

        var payments = app.MapGroup("payments");
        payments.MapPayments();

        var users = app.MapGroup("users");
        users.MapUsers();

        var members = app.MapGroup("members");
        members.MapMembers();

        return app;
    }
}
