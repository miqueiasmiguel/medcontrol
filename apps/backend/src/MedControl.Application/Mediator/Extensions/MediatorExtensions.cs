using System.Reflection;
using FluentValidation;
using MedControl.Application.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace MedControl.Application.Mediator.Extensions;

public static class MediatorExtensions
{
    /// <summary>
    /// Registers the custom Mediator, all handlers, validators and pipeline behaviors
    /// found in the provided assemblies.
    /// Pipeline order: Logging -> Validation -> Transaction -> Handler
    /// </summary>
    public static IServiceCollection AddMediator(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.AddScoped<IMediator, Mediator>();

        foreach (var assembly in assemblies)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t is { IsAbstract: false, IsInterface: false })
                .SelectMany(t => t.GetInterfaces(), (t, i) => (Type: t, Interface: i))
                .Where(x =>
                    x.Interface.IsGenericType &&
                    x.Interface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

            foreach (var (type, iface) in handlerTypes)
            {
                services.AddScoped(iface, type);
            }

            var eventHandlerTypes = assembly.GetTypes()
                .Where(t => t is { IsAbstract: false, IsInterface: false })
                .SelectMany(t => t.GetInterfaces(), (t, i) => (Type: t, Interface: i))
                .Where(x =>
                    x.Interface.IsGenericType &&
                    x.Interface.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>));

            foreach (var (type, iface) in eventHandlerTypes)
            {
                services.AddScoped(iface, type);
            }

            services.AddValidatorsFromAssembly(assembly, ServiceLifetime.Scoped);
        }

        // Pipeline order matters: registration order = execution order
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        return services;
    }
}
