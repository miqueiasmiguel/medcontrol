using MedControl.Infrastructure.Persistence;
using MedControl.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging.Abstractions;

namespace MedControl.Infrastructure.Tests.Helpers;

public sealed class DbContextModelFixture
{
    public IModel Model { get; }

    public DbContextModelFixture()
    {
        var fakeCurrentUser = new FakeCurrentUserService();
        var fakeServiceProvider = new FakeServiceProvider();

        var auditInterceptor = new AuditableEntityInterceptor(fakeCurrentUser);
        var domainEventInterceptor = new DomainEventDispatchInterceptor(
            fakeServiceProvider,
            NullLogger<DomainEventDispatchInterceptor>.Instance);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=fake")
            .Options;

        using var ctx = new ApplicationDbContext(
            options,
            auditInterceptor,
            domainEventInterceptor,
            fakeCurrentUser);

        Model = ctx.Model;
    }
}
