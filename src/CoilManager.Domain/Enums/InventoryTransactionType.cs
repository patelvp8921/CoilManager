namespace CoilManager.Domain.Enums;

public enum InventoryTransactionType
{
    SlittingJobRelease = 1,
    SlittingJobCancel = 2,
    SlittingJobComplete = 3,
    SlitCoilGeneration = 4,
    SlittingStarted = 5,
    LaminationAllocationReserved = 6,
    LaminationAllocationReleased = 7,
    LaminationConsumption = 8,
    Dispatch = 9
}
