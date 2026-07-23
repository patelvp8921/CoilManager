using CoilManager.Application.Interfaces.Services;

namespace CoilManager.UnitTests.DemoData;

public sealed class DemoDataContractTests
{
    [Fact]
    public void Generate_command_defaults_to_safe_non_destructive_full_generation()
    {
        var command = new GenerateDemoDataCommand();

        Assert.False(command.ClearExistingData);
        Assert.Equal("All", command.Stage);
    }

    [Fact]
    public void Summary_preserves_generation_counts()
    {
        var summary = new DemoDataSummary(50, 30, 280, 30, 18, 350, 2026, "complete");

        Assert.Equal(50, summary.MotherCoilsCreated);
        Assert.Equal(30, summary.SlittingJobsCreated);
        Assert.InRange(summary.SlitCoilsCreated, 250, 300);
        Assert.Equal("complete", summary.Message);
    }
}