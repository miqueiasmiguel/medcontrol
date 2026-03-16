using Microsoft.Extensions.DependencyInjection;

namespace MedControl.Application.Mediator;

internal sealed class Mediator(IServiceProvider serviceProvider) : IMediator
{
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var responseType = typeof(TResponse);

        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
        var handler = serviceProvider.GetRequiredService(handlerType);

        var behaviors = serviceProvider
            .GetServices(typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType))
            .Cast<object>()
            .ToList();

        Task<TResponse> HandlerDelegate() =>
            (Task<TResponse>)handlerType
                .GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle))!
                .Invoke(handler, [request, ct])!;

        if (behaviors.Count == 0)
        {
            return await HandlerDelegate();
        }

        RequestHandlerDelegate<TResponse> pipeline = HandlerDelegate;

        foreach (var behavior in Enumerable.Reverse(behaviors))
        {
            var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
            var current = pipeline;
            pipeline = () =>
                (Task<TResponse>)behaviorType
                    .GetMethod(nameof(IPipelineBehavior<IRequest<TResponse>, TResponse>.Handle))!
                    .Invoke(behavior, [request, current, ct])!;
        }

        return await pipeline();
    }
}
