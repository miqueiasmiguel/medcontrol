using MedControl.Application.Common.Interfaces;
using MedControl.Domain.Users;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

namespace MedControl.Api.Tests.Helpers;

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public IMagicLinkService MagicLinkService { get; } = Substitute.For<IMagicLinkService>();
    public IEmailService EmailService { get; } = Substitute.For<IEmailService>();
    public ITokenService TokenService { get; } = Substitute.For<ITokenService>();
    public IUserRepository UserRepository { get; } = Substitute.For<IUserRepository>();
    public IUnitOfWork UnitOfWork { get; } = Substitute.For<IUnitOfWork>();
    public IGoogleAuthService GoogleAuthService { get; } = Substitute.For<IGoogleAuthService>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "test-secret-key-long-enough-for-hmac-sha256-algorithm",
                ["Jwt:Issuer"] = "https://test.medcontrol.app",
                ["Jwt:Audience"] = "medcontrol-api",
                ["ConnectionStrings:Database"] = "Host=localhost;Database=test",
                ["ConnectionStrings:Redis"] = "localhost",
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IMagicLinkService>();
            services.RemoveAll<IEmailService>();
            services.RemoveAll<ITokenService>();
            services.RemoveAll<IUserRepository>();
            services.RemoveAll<IUnitOfWork>();
            services.RemoveAll<IDistributedCache>();
            services.RemoveAll<IGoogleAuthService>();

            services.AddSingleton(MagicLinkService);
            services.AddSingleton(EmailService);
            services.AddSingleton(TokenService);
            services.AddSingleton(UserRepository);
            services.AddSingleton(UnitOfWork);
            services.AddSingleton<IDistributedCache>(Substitute.For<IDistributedCache>());
            services.AddSingleton(GoogleAuthService);
        });
    }
}
