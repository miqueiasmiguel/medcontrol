namespace MedControl.Application.Mediator;

/// <summary>Query that returns a value. Queries must not cause side effects.</summary>
public interface IQuery<out TResponse> : IRequest<TResponse>;
