using MedControl.Application.Mediator.Extensions;
using MedControl.Infrastructure.Extensions;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddMediator(Assembly.GetAssembly(typeof(MedControl.Application.Mediator.IMediator))!);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

// Expose for WebApplicationFactory in integration tests
public partial class Program;
