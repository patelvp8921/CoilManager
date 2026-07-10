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
    public static string Generate(string slittingJobNo, int sequenceNo)
    {
        string suffix = sequenceNo.ToString("00");
        string normalizedJobNo = slittingJobNo.Replace('/', '-');
        return normalizedJobNo.StartsWith("AE-S-", StringComparison.OrdinalIgnoreCase)
            ? $"SC-{normalizedJobNo[5..]}-{suffix}"
            : $"SC-{normalizedJobNo}-{suffix}";
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

    public static decimal EstimateWeight(
        decimal slitWidth,
        decimal motherCoilWidth,
        decimal motherCoilWeight,
        decimal thickness,
        decimal length)
    {
        if (slitWidth <= 0)
        {
            return 0;
        }

        if (motherCoilWidth > 0 && motherCoilWeight > 0)
        {
            return Math.Round(motherCoilWeight * slitWidth / motherCoilWidth, 3);
        }

        return EstimateWeight(slitWidth, thickness, length);
    }

    private static decimal NormalizeLengthToMillimeters(decimal length)
    {
        return length < MeterLikeLengthThreshold
            ? length * MillimetersPerMeter
            : length;
    }
}
