using CoilManager.Shared.Exceptions;

namespace CoilManager.Application.Services;

public static class MaterialAllocationCalculator
{
    public static decimal AvailableWeight(decimal currentWeight, decimal activeReservedWeight)
        => Math.Max(0, currentWeight - activeReservedWeight);

    public static decimal ValidateAndCalculateRemaining(decimal currentWeight, decimal activeReservedWeight, decimal requestedWeight)
    {
        if (requestedWeight <= 0) throw new BusinessRuleException("Allocated Weight must be greater than zero.");
        decimal available = AvailableWeight(currentWeight, activeReservedWeight);
        if (requestedWeight > available) throw new BusinessRuleException($"Allocated Weight exceeds the available weight of {available:N3} kg.");
        return available - requestedWeight;
    }
}
