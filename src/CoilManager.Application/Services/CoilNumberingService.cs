using CoilManager.Application.Interfaces.Services;

namespace CoilManager.Application.Services;

public sealed class CoilNumberingService : ICoilNumberingService
{
    public string GenerateFirstGenerationSlitCoilNumber(string motherCoilNumber, int slitSequence)
    {
        string suffix = slitSequence.ToString("00");
        return motherCoilNumber.StartsWith("MC-", StringComparison.OrdinalIgnoreCase)
            ? $"SC-{motherCoilNumber[3..]}-{suffix}"
            : $"SC-{motherCoilNumber}-{suffix}";
    }

    public string GenerateChildSlitCoilNumber(string parentCoilNumber, int slitSequence)
    {
        return $"{parentCoilNumber}-{slitSequence:00}";
    }
}
