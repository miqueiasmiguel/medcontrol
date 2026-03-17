using MedControl.Api.Endpoints;
using MedControl.Api.Extensions;
using MedControl.Application.Mediator.Extensions;
using MedControl.Infrastructure.Extensions;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddMediator(Assembly.GetAssembly(typeof(MedControl.Application.Mediator.IMediator))!);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseApiExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapApiEndpoints();

app.Run();

public partial class Program;
