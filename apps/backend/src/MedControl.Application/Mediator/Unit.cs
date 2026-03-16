namespace MedControl.Application.Mediator;

/// <summary>Represents the absence of a value (void equivalent for async operations).</summary>
public readonly record struct Unit
{
    public static readonly Unit Value;
}
