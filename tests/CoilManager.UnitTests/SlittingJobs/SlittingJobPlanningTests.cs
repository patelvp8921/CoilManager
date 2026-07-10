using CoilManager.Application.DTOs.SlittingJobs;
using CoilManager.Application.Services;
using CoilManager.Application.Validators.SlittingJobs;
using CoilManager.Domain.Entities;
using CoilManager.Domain.Enums;

namespace CoilManager.UnitTests.SlittingJobs;

public sealed class SlittingJobPlanningTests
{
    [Fact]
    public void JobNumberGenerator_UsesRequestedFormat()
    {
        string jobNumber = SlittingJobNumberGenerator.Generate(2026, 1);

        Assert.Equal("AE/S/2026/00001", jobNumber);
    }

    [Fact]
    public void SlitCoilIdGenerator_UsesJobNumberSequence()
    {
        string slitCoilId = SlitCoilIdGenerator.Generate("AE/S/2026/00002", 6);

        Assert.Equal("SC-2026-00002-06", slitCoilId);
    }

    [Fact]
    public void Calculator_ComputesWidthSummary()
    {
        SlittingPlanningSummary summary = SlittingPlanningCalculator.Calculate(
            1250m,
            [200m, 300m, 250m],
            0.2m,
            5m,
            5m);

        Assert.Equal(750m, summary.TotalPlannedWidth);
        Assert.Equal(0.4m, summary.KnifeLoss);
        Assert.Equal(10m, summary.EdgeTrim);
        Assert.Equal(489.6m, summary.RemainingWidth);
        Assert.Equal(60m, summary.UtilizationPercent);
        Assert.Equal(2, summary.NumberOfCuts);
    }

    [Fact]
    public void Calculator_EstimatesCrgoWeightFromDimensions()
    {
        decimal estimatedWeight = SlittingPlanningCalculator.EstimateWeight(
            slitWidth: 200m,
            thickness: 0.27m,
            length: 100_000m);

        Assert.Equal(41.310m, estimatedWeight);
    }

    [Fact]
    public void Calculator_TreatsSmallLengthValuesAsMeters()
    {
        decimal estimatedWeight = SlittingPlanningCalculator.EstimateWeight(
            slitWidth: 200m,
            thickness: 0.27m,
            length: 100m);

        Assert.Equal(41.310m, estimatedWeight);
    }

    [Fact]
    public void Calculator_UsesMotherCoilWeightWhenLengthIsMissing()
    {
        decimal estimatedWeight = SlittingPlanningCalculator.EstimateWeight(
            slitWidth: 250m,
            motherCoilWidth: 1250m,
            motherCoilWeight: 10m,
            thickness: 0.23m,
            length: 0m);

        Assert.Equal(2.000m, estimatedWeight);
    }

    [Fact]
    public void Release_ChangesJobAndItemStatuses()
    {
        SlittingJob job = new(
            "AE/S/2026/00001",
            DateOnly.FromDateTime(DateTime.UtcNow),
            null,
            Guid.NewGuid(),
            null,
            "A",
            0.2m,
            5m,
            5m,
            null);
        job.ReplaceItems([
            new SlittingJobItem(1, "SC-2026-00001-01", 200m, 100m),
            new SlittingJobItem(2, "SC-2026-00001-02", 300m, 150m)
        ]);

        job.Release();

        Assert.Equal(SlittingJobStatus.Released, job.Status);
        Assert.All(job.Items, item => Assert.Equal(SlittingJobStatus.Released, item.Status));
    }

    [Fact]
    public void ReplaceItems_UpdatesExistingDraftRows()
    {
        SlittingJob job = new(
            "AE/S/2026/00001",
            DateOnly.FromDateTime(DateTime.UtcNow),
            null,
            Guid.NewGuid(),
            null,
            "A",
            0.2m,
            5m,
            5m,
            null);
        SlittingJobItem firstItem = new(1, "SC-2026-00001-01", 200m, 100m);
        SlittingJobItem secondItem = new(2, "SC-2026-00001-02", 300m, 150m);
        Guid firstItemId = firstItem.Id;
        Guid secondItemId = secondItem.Id;
        job.ReplaceItems([firstItem, secondItem]);

        job.ReplaceItems([
            new SlittingJobItem(1, "SC-2026-00001-01", 220m, 110m, "Updated"),
            new SlittingJobItem(2, "SC-2026-00001-02", 280m, 140m)
        ]);

        SlittingJobItem[] items = job.Items.OrderBy(item => item.SequenceNo).ToArray();
        Assert.Equal(firstItemId, items[0].Id);
        Assert.Equal(secondItemId, items[1].Id);
        Assert.Equal(220m, items[0].Width);
        Assert.Equal(110m, items[0].EstimatedWeight);
        Assert.Equal("Updated", items[0].Remarks);
    }

    [Fact]
    public async Task CreateValidator_RejectsNonSequentialRows()
    {
        CreateSlittingJobRequest request = ValidCreateRequest() with
        {
            Items =
            [
                new SlittingJobItemRequest(1, 200m, null),
                new SlittingJobItemRequest(3, 200m, null)
            ]
        };
        CreateSlittingJobRequestValidator validator = new();

        FluentValidation.Results.ValidationResult result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("sequential", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CreateValidator_RejectsMissingItems()
    {
        CreateSlittingJobRequest request = ValidCreateRequest() with { Items = [] };
        CreateSlittingJobRequestValidator validator = new();

        FluentValidation.Results.ValidationResult result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateSlittingJobRequest.Items));
    }

    [Fact]
    public async Task CreateValidator_RejectsMoreThanTenRows()
    {
        CreateSlittingJobRequest request = ValidCreateRequest() with
        {
            Items = Enumerable.Range(1, 11)
                .Select(sequence => new SlittingJobItemRequest(sequence, 100m, null))
                .ToArray()
        };
        CreateSlittingJobRequestValidator validator = new();

        FluentValidation.Results.ValidationResult result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("Maximum 10", StringComparison.OrdinalIgnoreCase));
    }

    private static CreateSlittingJobRequest ValidCreateRequest()
    {
        return new CreateSlittingJobRequest(
            DateOnly.FromDateTime(DateTime.UtcNow),
            "Planner",
            Guid.NewGuid(),
            null,
            "A",
            0.2m,
            5m,
            5m,
            null,
            [
                new SlittingJobItemRequest(1, 200m, null),
                new SlittingJobItemRequest(2, 300m, null)
            ]);
    }
}
