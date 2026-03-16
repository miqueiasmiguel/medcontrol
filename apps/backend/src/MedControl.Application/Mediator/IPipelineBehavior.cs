namespace MedControl.Application.Mediator;

public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct);
}

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();
