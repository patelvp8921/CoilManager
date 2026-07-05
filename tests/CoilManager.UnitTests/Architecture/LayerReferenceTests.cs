using CoilManager.Application;
using CoilManager.Domain.Common;
using CoilManager.Shared;

namespace CoilManager.UnitTests.Architecture;

public sealed class LayerReferenceTests
{
    [Fact]
    public void CoreAssemblies_AreLoadable()
    {
        Assert.NotNull(typeof(DependencyInjection).Assembly);
        Assert.NotNull(typeof(BaseEntity).Assembly);
        Assert.NotNull(typeof(AssemblyReference).Assembly);
    }
}
