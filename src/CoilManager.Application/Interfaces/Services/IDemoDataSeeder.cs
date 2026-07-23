namespace CoilManager.Application.Interfaces.Services;

public interface IDemoDataSeeder
{
    Task<DemoDataSummary> GenerateAsync(GenerateDemoDataCommand command, CancellationToken token = default);
}

public sealed record GenerateDemoDataCommand(bool ClearExistingData = false, string Stage = "All");
public sealed record DemoDataSummary(int MotherCoilsCreated,int SlittingJobsCreated,int SlitCoilsCreated,int LaminationJobsCreated,int MaterialAllocationsCreated,int InventoryTransactionsCreated,long ElapsedMilliseconds,string Message);