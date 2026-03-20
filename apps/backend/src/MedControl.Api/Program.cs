using MedControl.Api.Endpoints;
using MedControl.Api.Extensions;
using MedControl.Application.Mediator.Extensions;
using MedControl.Infrastructure.Extensions;
using Scalar.AspNetCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddMediator(Assembly.GetAssembly(typeof(MedControl.Application.Mediator.IMediator))!);
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

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
app.MapApiEndpoints();

app.Run();

// required for WebApplicationFactory in integration tests
public partial class Program;
