using System.Reflection;
using NetArchTest.Rules;

namespace MedControl.Architecture.Tests;

public sealed class ArchitectureTests
{
    private static readonly Assembly DomainAssembly =
        typeof(MedControl.Domain.Common.BaseEntity).Assembly;

    private static readonly Assembly ApplicationAssembly =
        typeof(MedControl.Application.Mediator.IMediator).Assembly;

    private static readonly Assembly InfrastructureAssembly =
        typeof(MedControl.Infrastructure.Persistence.ApplicationDbContext).Assembly;

    // Program is a top-level type in the API project (no namespace)
    private static readonly Assembly ApiAssembly =
        typeof(Program).Assembly;

    [Fact]
    public void Domain_Should_Not_HaveDependencyOn_Application()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("MedControl.Application")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Domain_Should_Not_HaveDependencyOn_Infrastructure()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("MedControl.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Domain_Should_Not_HaveDependencyOn_Api()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("MedControl.Api")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Application_Should_Not_HaveDependencyOn_Infrastructure()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn("MedControl.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Application_Should_Not_HaveDependencyOn_Api()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn("MedControl.Api")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Infrastructure_Should_Not_HaveDependencyOn_Api()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn("MedControl.Api")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Handlers_Should_Be_Sealed()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(MedControl.Application.Mediator.IRequestHandler<,>))
            .Should()
            .BeSealed()
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void DomainEntities_Should_Have_PrivateParameterlessConstructor()
    {
        var entityTypes = DomainAssembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(MedControl.Domain.Common.BaseEntity)) && !t.IsAbstract);

        foreach (var type in entityTypes)
        {
            var privateCtor = type.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                Type.EmptyTypes);

            Assert.True(
                privateCtor is not null,
                $"{type.Name} must have a private parameterless constructor for EF Core.");
        }
    }

    private static string FormatFailures(TestResult result) =>
        string.Join(", ", result.FailingTypeNames ?? []);
}
