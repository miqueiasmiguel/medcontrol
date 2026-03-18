using MedControl.Application.Common.Interfaces;
using MedControl.Domain.Doctors;
using MedControl.Domain.Tenants;
using MedControl.Domain.Users;
using Microsoft.AspNetCore.Authentication;
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
    public ITenantRepository TenantRepository { get; } = Substitute.For<ITenantRepository>();
    public IDoctorRepository DoctorRepository { get; } = Substitute.For<IDoctorRepository>();

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
                ["Cors:WebOrigin"] = "http://localhost:4200",
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
            services.RemoveAll<ITenantRepository>();
            services.RemoveAll<IDoctorRepository>();

            services.AddSingleton(MagicLinkService);
            services.AddSingleton(EmailService);
            services.AddSingleton(TokenService);
            services.AddSingleton(UserRepository);
            services.AddSingleton(UnitOfWork);
            services.AddSingleton<IDistributedCache>(Substitute.For<IDistributedCache>());
            services.AddSingleton(GoogleAuthService);
            services.AddSingleton(TenantRepository);
            services.AddSingleton(DoctorRepository);

            // Replace JWT auth with a test handler that reads from X-Test-* headers
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            });
        });
    }

    /// <summary>
    /// Creates a client that authenticates as the given user via X-Test-* headers.
    /// </summary>
    public HttpClient CreateAuthenticatedClient(Guid userId, string email, Guid? tenantId = null)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false,
        });
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.EmailHeader, email);
        if (tenantId.HasValue)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.TenantIdHeader, tenantId.Value.ToString());
        }
        return client;
    }
}
