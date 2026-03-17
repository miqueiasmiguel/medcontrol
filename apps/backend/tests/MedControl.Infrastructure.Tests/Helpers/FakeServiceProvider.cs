namespace MedControl.Infrastructure.Tests.Helpers;

internal sealed class FakeServiceProvider : IServiceProvider
{
    public object? GetService(Type serviceType) => null;
}
