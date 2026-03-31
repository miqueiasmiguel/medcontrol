using MedControl.Api.Endpoints;
using MedControl.Api.Extensions;
using MedControl.Application.Mediator.Extensions;
using MedControl.Infrastructure.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddMediator(Assembly.GetAssembly(typeof(MedControl.Application.Mediator.IMediator))!);
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddApiServices(builder.Configuration);

builder.Services.AddProdSeed();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDevSeed();
}

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var prodSeeder = scope.ServiceProvider.GetRequiredService<MedControl.Infrastructure.Persistence.Seeding.ProdDataSeeder>();
    await prodSeeder.SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<MedControl.Infrastructure.Persistence.Seeding.DevDataSeeder>();
    await seeder.SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Trust X-Forwarded-Proto from Caddy so IsHttps is correct behind the Cloudflare → Caddy proxy.
// KnownNetworks/KnownProxies cleared to trust any upstream (Caddy is in the same Docker network).
var forwardedOptions = new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedProto };
forwardedOptions.KnownIPNetworks.Clear();
forwardedOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedOptions);

app.UseApiExceptionHandler();
app.UseCors("WebApp");
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
    Secure = CookieSecurePolicy.SameAsRequest,
});
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString().ToLowerInvariant(),
            checks = report.Entries.ToDictionary(
                e => e.Key,
                e => e.Value.Status.ToString().ToLowerInvariant()),
        });
    },
});
app.MapApiEndpoints();

app.Run();

// required for WebApplicationFactory in integration tests
public partial class Program;
