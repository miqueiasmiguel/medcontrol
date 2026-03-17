using MedControl.Application.Common.Interfaces;
using MedControl.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace MedControl.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used exclusively by EF Core tooling (migrations, scaffolding).
/// Not used at runtime.
/// </summary>
internal sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "MedControl.Api"))
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Database")
            ?? "Host=localhost;Port=5432;Database=medcontrol;Username=medcontrol;Password=medcontrol";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
            .Options;

        var fakeCurrentUser = new DesignTimeFakeCurrentUserService();
        var auditInterceptor = new AuditableEntityInterceptor(fakeCurrentUser);
        var domainEventInterceptor = new DomainEventDispatchInterceptor(
            new DesignTimeFakeServiceProvider(),
            NullLogger<DomainEventDispatchInterceptor>.Instance);

        return new ApplicationDbContext(options, auditInterceptor, domainEventInterceptor, fakeCurrentUser);
    }

    private sealed class DesignTimeFakeCurrentUserService : ICurrentUserService
    {
        public Guid? UserId => null;
        public Guid? TenantId => null;
        public string? Email => null;
        public IReadOnlyList<string> Roles => [];
        public IReadOnlyList<string> GlobalRoles => [];
        public bool IsAuthenticated => false;
        public bool HasGlobalRole(string role) => false;
    }

    private sealed class DesignTimeFakeServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
