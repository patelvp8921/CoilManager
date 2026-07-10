namespace CoilManager.Application.Services;

public static class SlittingJobNumberGenerator
{
    public static string Generate(int year, int sequence)
    {
        return $"AE/S/{year}/{sequence:00000}";
    }
}

public static class SlitCoilIdGenerator
{
    public static string Generate(string motherCoilId, int sequenceNo)
    {
        string suffix = sequenceNo.ToString("00");
        return motherCoilId.StartsWith("MC-", StringComparison.OrdinalIgnoreCase)
            ? $"SC-{motherCoilId[3..]}-{suffix}"
            : $"SC-{motherCoilId}-{suffix}";
    }
}

public sealed record SlittingPlanningSummary(
    decimal TotalPlannedWidth,
    decimal KnifeLoss,
    decimal EdgeTrim,
    decimal RemainingWidth,
    decimal UtilizationPercent,
    int NumberOfCuts);

public static class SlittingPlanningCalculator
{
    private const decimal CrgoDensityKgPerCubicMeter = 7650m;
    private const decimal CubicMillimetersPerCubicMeter = 1_000_000_000m;
    private const decimal MeterLikeLengthThreshold = 10_000m;
    private const decimal MillimetersPerMeter = 1_000m;

    public static SlittingPlanningSummary Calculate(
        decimal motherCoilWidth,
        IEnumerable<decimal> slitWidths,
        decimal knifeThickness,
        decimal leftEdgeTrim,
        decimal rightEdgeTrim)
    {
        decimal[] widths = slitWidths.ToArray();
        decimal totalPlannedWidth = widths.Sum();
        int numberOfCuts = Math.Max(widths.Length - 1, 0);
        decimal knifeLoss = numberOfCuts * knifeThickness;
        decimal edgeTrim = leftEdgeTrim + rightEdgeTrim;
        decimal consumedWidth = totalPlannedWidth + knifeLoss + edgeTrim;
        decimal remainingWidth = motherCoilWidth - consumedWidth;
        decimal utilizationPercent = motherCoilWidth <= 0
            ? 0
            : Math.Round(totalPlannedWidth / motherCoilWidth * 100m, 2);

        return new SlittingPlanningSummary(
            totalPlannedWidth,
            knifeLoss,
            edgeTrim,
            remainingWidth,
            utilizationPercent,
            numberOfCuts);
    }

    public static decimal EstimateWeight(decimal slitWidth, decimal thickness, decimal length)
    {
        if (slitWidth <= 0 || thickness <= 0 || length <= 0)
        {
            return 0;
        }

        decimal lengthInMillimeters = NormalizeLengthToMillimeters(length);
        return Math.Round(slitWidth * thickness * lengthInMillimeters * CrgoDensityKgPerCubicMeter / CubicMillimetersPerCubicMeter, 3);
    }

    private static decimal NormalizeLengthToMillimeters(decimal length)
    {
        return length < MeterLikeLengthThreshold
            ? length * MillimetersPerMeter
            : length;
    }
}
