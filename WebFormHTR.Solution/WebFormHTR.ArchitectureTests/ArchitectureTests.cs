using NetArchTest.Rules;
using Xunit;
using WebFormHTR.Domain.Entities.Base;
using WebFormHTR.Application;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.API.Endpoints;

namespace WebFormHTR.ArchitectureTests;

public class ArchitectureTests
{
    private const string DomainNamespace = "WebFormHTR.Domain";
    private const string ApplicationNamespace = "WebFormHTR.Application";
    private const string InfrastructureNamespace = "WebFormHTR.Infrastructure";
    private const string ApiNamespace = "WebFormHTR.API";

    [Fact]
    public void Domain_Should_Not_HaveDependencyOnOtherLayers()
    {
        var assembly = typeof(BaseEntity).Assembly;

        AssertLayerDoesNotDependOn(assembly, ApplicationNamespace, "Application");
        AssertLayerDoesNotDependOn(assembly, InfrastructureNamespace, "Infrastructure");
        AssertLayerDoesNotDependOn(assembly, ApiNamespace, "API");
    }

    [Fact]
    public void Application_Should_Not_HaveDependencyOnInfrastructure_Or_Api()
    {
        var assembly = typeof(ApplicationAssemblyReference).Assembly;

        AssertLayerDoesNotDependOn(assembly, InfrastructureNamespace, "Infrastructure");
        AssertLayerDoesNotDependOn(assembly, ApiNamespace, "API");
    }

    [Fact]
    public void Infrastructure_Should_Not_HaveDependencyOnApi()
    {
        var assembly = typeof(AppDbContext).Assembly;

        AssertLayerDoesNotDependOn(assembly, ApiNamespace, "API");
    }

    private void AssertLayerDoesNotDependOn(System.Reflection.Assembly assembly, string dependencyNamespace, string dependencyName)
    {
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn(dependencyNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, $"{assembly.GetName().Name} should not depend on {dependencyName}");
    }
}
