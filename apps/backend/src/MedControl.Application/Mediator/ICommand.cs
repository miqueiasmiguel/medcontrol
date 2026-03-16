namespace MedControl.Application.Mediator;

/// <summary>Command with no return value.</summary>
public interface ICommand : IRequest<Unit>;

/// <summary>Command with a return value.</summary>
public interface ICommand<out TResponse> : IRequest<TResponse>;
