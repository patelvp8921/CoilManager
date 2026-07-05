using System.Reflection;
using ApplicationAssembly = CoilManager.Application.AssemblyReference;
using DomainAssembly = CoilManager.Domain.AssemblyReference;
using SharedAssembly = CoilManager.Shared.AssemblyReference;

namespace CoilManager.UnitTests.Architecture;

public sealed class LayerReferenceTests
{
    [Fact]
    public void CoreAssemblies_AreLoadable()
    {
        Assert.NotNull(typeof(ApplicationAssembly).Assembly);
        Assert.NotNull(typeof(DomainAssembly).Assembly);
        Assert.NotNull(typeof(SharedAssembly).Assembly);
    }

    [Fact]
    public void Domain_DoesNotReferenceApplicationInfrastructurePersistenceOrApi()
    {
        Assembly domainAssembly = typeof(DomainAssembly).Assembly;

        Assert.DoesNotContain(domainAssembly.GetReferencedAssemblies(), reference =>
            reference.Name is "CoilManager.Application"
                or "CoilManager.Infrastructure"
                or "CoilManager.Persistence"
                or "CoilManager.API");
    }

    [Fact]
    public void Application_DoesNotReferenceInfrastructurePersistenceOrApi()
    {
        Assembly applicationAssembly = typeof(ApplicationAssembly).Assembly;

        Assert.DoesNotContain(applicationAssembly.GetReferencedAssemblies(), reference =>
            reference.Name is "CoilManager.Infrastructure"
                or "CoilManager.Persistence"
                or "CoilManager.API");
    }
}
