using MedControl.Application.Auth.Settings;
using MedControl.Application.Common.Interfaces;
using MedControl.Domain.Doctors;
using MedControl.Domain.Tenants;
using MedControl.Domain.Users;
using MedControl.Infrastructure.Auth;
using MedControl.Infrastructure.Auth.Settings;
using MedControl.Infrastructure.Http;
using MedControl.Infrastructure.Persistence;
using MedControl.Infrastructure.Persistence.Interceptors;
using MedControl.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Resend;

namespace MedControl.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // HttpContext — fixes DI bug: ApplicationDbContext requires ICurrentUserService
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();
        services.AddScoped<ICurrentTenantService, HttpContextCurrentTenantService>();

        // Settings
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<MagicLinkSettings>(configuration.GetSection(MagicLinkSettings.SectionName));
        services.Configure<GoogleAuthSettings>(configuration.GetSection(GoogleAuthSettings.SectionName));

        // Redis
        services.AddStackExchangeRedisCache(opts =>
            opts.Configuration = configuration.GetConnectionString("Redis"));

        // Auth services
        services.AddScoped<IMagicLinkService, MagicLinkService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddHttpClient<IGoogleAuthService, GoogleAuthService>();

        if (environment.IsDevelopment())
        {
            services.AddScoped<IEmailService, DevelopmentEmailService>();
        }
        else
        {
            // Resend
            services.AddHttpClient<ResendClient>();
            services.Configure<ResendClientOptions>(opts =>
                opts.ApiToken = configuration["Resend:ApiKey"]!);
            services.AddTransient<IResend, ResendClient>();

            services.AddScoped<IEmailService, EmailService>();
        }

        // Persistence
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<DomainEventDispatchInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("Database"),
                npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();

        return services;
    }
}
