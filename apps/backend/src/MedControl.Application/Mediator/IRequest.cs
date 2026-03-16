namespace MedControl.Application.Mediator;

/// <summary>Marker interface for all requests (commands and queries).</summary>
public interface IRequest<out TResponse>;
