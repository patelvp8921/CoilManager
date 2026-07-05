using System.Reflection;
using InfrastructureAssembly = CoilManager.Infrastructure.AssemblyReference;
using PersistenceAssembly = CoilManager.Persistence.AssemblyReference;

namespace CoilManager.IntegrationTests.Architecture;

public sealed class OuterLayerReferenceTests
{
    [Fact]
    public void OuterLayerAssemblies_AreLoadable()
    {
        Assert.NotNull(typeof(Program).Assembly);
        Assert.NotNull(typeof(InfrastructureAssembly).Assembly);
        Assert.NotNull(typeof(PersistenceAssembly).Assembly);
    }

    [Fact]
    public void Infrastructure_DoesNotReferencePersistenceOrApi()
    {
        Assembly infrastructureAssembly = typeof(InfrastructureAssembly).Assembly;

        Assert.DoesNotContain(infrastructureAssembly.GetReferencedAssemblies(), reference =>
            reference.Name is "CoilManager.Persistence" or "CoilManager.API");
    }

    [Fact]
    public void Persistence_DoesNotReferenceInfrastructureOrApi()
    {
        Assembly persistenceAssembly = typeof(PersistenceAssembly).Assembly;

        Assert.DoesNotContain(persistenceAssembly.GetReferencedAssemblies(), reference =>
            reference.Name is "CoilManager.Infrastructure" or "CoilManager.API");
    }
}
