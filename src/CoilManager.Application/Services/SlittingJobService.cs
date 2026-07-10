using CoilManager.Application.DTOs.SlittingJobs;
using CoilManager.Application.Interfaces.Persistence;
using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Application.Mappings;
using CoilManager.Domain.Entities;
using CoilManager.Domain.Enums;
using CoilManager.Shared.Errors;
using CoilManager.Shared.Pagination;
using CoilManager.Shared.Results;
using FluentValidation;

namespace CoilManager.Application.Services;

public sealed class SlittingJobService(
    ISlittingJobRepository slittingJobRepository,
    IRawCoilRepository rawCoilRepository,
    IUnitOfWork unitOfWork,
    IValidator<CreateSlittingJobRequest> createValidator,
    IValidator<UpdateSlittingJobRequest> updateValidator) : ISlittingJobService
{
    private const decimal DefaultKnifeThickness = 0.2m;
    private const decimal DefaultLeftEdgeTrim = 5m;
    private const decimal DefaultRightEdgeTrim = 5m;

    public Task<PagedResult<SlittingJobDto>> GetAsync(SlittingJobQueryRequest request, CancellationToken cancellationToken = default)
    {
        return slittingJobRepository.GetPagedAsync(request, cancellationToken);
    }

    public async Task<Result<SlittingJobDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        SlittingJob? job = await slittingJobRepository.GetByIdAsync(id, cancellationToken);
        if (job is null)
        {
            return Result<SlittingJobDto>.Failure(Error.NotFound($"Slitting job '{id}' was not found."));
        }

        return Result<SlittingJobDto>.Success(SlittingJobDtoMapper.MapToDto(job));
    }

    public Task<string> GetNextJobNumberAsync(CancellationToken cancellationToken = default)
    {
        return BuildNextJobNumberAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SlittingMotherCoilLookupDto>> SearchMotherCoilsAsync(string? search, CancellationToken cancellationToken = default)
    {
        string? normalizedSearch = Normalize(search);
        IReadOnlyList<RawCoil> rawCoils = await rawCoilRepository.GetAllAsync(cancellationToken);

        return rawCoils
            .Where(rawCoil => rawCoil.Status == CoilStatus.Available)
            .Where(rawCoil => normalizedSearch is null
                || rawCoil.RawCoilNumber.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase)
                || rawCoil.CoilNumber.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase)
                || rawCoil.HeatNumber.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(rawCoil => rawCoil.ReceivedDate)
            .ThenByDescending(rawCoil => rawCoil.CreatedAtUtc)
            .Take(20)
            .Select(rawCoil => new SlittingMotherCoilLookupDto(
                rawCoil.Id,
                rawCoil.RawCoilNumber,
                rawCoil.CoilNumber,
                rawCoil.HeatNumber,
                rawCoil.Supplier?.Name,
                rawCoil.Manufacturer?.Name,
                rawCoil.Grade?.Code,
                rawCoil.Thickness,
                rawCoil.Category,
                rawCoil.CoreLossPerKg,
                rawCoil.Width,
                rawCoil.Weight,
                rawCoil.Length,
                rawCoil.WarehouseLocation,
                rawCoil.Status))
            .ToArray();
    }

    public async Task<Result<SlittingJobDto>> CreateAsync(CreateSlittingJobRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        FluentValidation.Results.ValidationResult validation = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<SlittingJobDto>.Failure(Error.Validation(string.Join("; ", validation.Errors.Select(error => error.ErrorMessage))));
        }

        RawCoil? motherCoil = await rawCoilRepository.GetByIdAsync(request.MotherCoilId, cancellationToken);
        if (motherCoil is null)
        {
            return Result<SlittingJobDto>.Failure(Error.Validation("Mother coil is required."));
        }

        Result widthValidation = ValidateWidth(motherCoil.Width, request.Items, request.KnifeThickness, request.LeftEdgeTrim, request.RightEdgeTrim);
        if (widthValidation.IsFailure)
        {
            return Result<SlittingJobDto>.Failure(widthValidation.Error);
        }

        string jobNumber = await BuildNextJobNumberAsync(cancellationToken);
        SlittingJob job = new(
            jobNumber,
            request.PlanningDate,
            request.PlannerId,
            request.MotherCoilId,
            request.MachineId,
            request.Shift,
            NormalizeParameter(request.KnifeThickness, DefaultKnifeThickness),
            NormalizeParameter(request.LeftEdgeTrim, DefaultLeftEdgeTrim),
            NormalizeParameter(request.RightEdgeTrim, DefaultRightEdgeTrim),
            request.Remarks);

        job.ReplaceItems(BuildItems(motherCoil, request.Items));

        await slittingJobRepository.AddAsync(job, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        SlittingJob? savedJob = await slittingJobRepository.GetByIdAsync(job.Id, cancellationToken);
        return Result<SlittingJobDto>.Success(SlittingJobDtoMapper.MapToDto(savedJob ?? job));
    }

    public async Task<Result<SlittingJobDto>> UpdateAsync(Guid id, UpdateSlittingJobRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        FluentValidation.Results.ValidationResult validation = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<SlittingJobDto>.Failure(Error.Validation(string.Join("; ", validation.Errors.Select(error => error.ErrorMessage))));
        }

        SlittingJob? job = await slittingJobRepository.GetByIdAsync(id, cancellationToken);
        if (job is null)
        {
            return Result<SlittingJobDto>.Failure(Error.NotFound($"Slitting job '{id}' was not found."));
        }

        if (job.Status != SlittingJobStatus.Draft)
        {
            return Result<SlittingJobDto>.Failure(Error.Validation("Only draft slitting jobs can be edited."));
        }

        if (!RowVersionMatches(job.RowVersion, request.RowVersion))
        {
            return Result<SlittingJobDto>.Failure(Error.Conflict("The slitting job was modified by another process. Reload and try again."));
        }

        RawCoil? motherCoil = await rawCoilRepository.GetByIdAsync(request.MotherCoilId, cancellationToken);
        if (motherCoil is null)
        {
            return Result<SlittingJobDto>.Failure(Error.Validation("Mother coil is required."));
        }

        Result widthValidation = ValidateWidth(motherCoil.Width, request.Items, request.KnifeThickness, request.LeftEdgeTrim, request.RightEdgeTrim);
        if (widthValidation.IsFailure)
        {
            return Result<SlittingJobDto>.Failure(widthValidation.Error);
        }

        IReadOnlyList<SlittingJobItem> rebuiltItems = BuildItems(motherCoil, request.Items);

        job.UpdatePlanningDetails(
            request.PlanningDate,
            request.PlannerId,
            request.MotherCoilId,
            request.MachineId,
            request.Shift,
            NormalizeParameter(request.KnifeThickness, DefaultKnifeThickness),
            NormalizeParameter(request.LeftEdgeTrim, DefaultLeftEdgeTrim),
            NormalizeParameter(request.RightEdgeTrim, DefaultRightEdgeTrim),
            request.Remarks);

        await slittingJobRepository.DeleteItemsForRebuildAsync(job, cancellationToken);
        job.RebuildItems(rebuiltItems);
        slittingJobRepository.TrackRebuiltItemsAsAdded(rebuiltItems);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        SlittingJob? savedJob = await slittingJobRepository.GetByIdAsync(job.Id, cancellationToken);
        return Result<SlittingJobDto>.Success(SlittingJobDtoMapper.MapToDto(savedJob ?? job));
    }

    public async Task<Result<SlittingJobDto>> ReleaseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        SlittingJob? job = await slittingJobRepository.GetByIdAsync(id, cancellationToken);
        if (job is null)
        {
            return Result<SlittingJobDto>.Failure(Error.NotFound($"Slitting job '{id}' was not found."));
        }

        if (job.Status != SlittingJobStatus.Draft)
        {
            return Result<SlittingJobDto>.Failure(Error.Validation("Only draft slitting jobs can be released."));
        }

        if (job.Items.Count == 0)
        {
            return Result<SlittingJobDto>.Failure(Error.Validation("At least one slit row is required before release."));
        }

        job.Release();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        SlittingJob? savedJob = await slittingJobRepository.GetByIdAsync(job.Id, cancellationToken);
        return Result<SlittingJobDto>.Success(SlittingJobDtoMapper.MapToDto(savedJob ?? job));
    }

    private static IReadOnlyList<SlittingJobItem> BuildItems(RawCoil motherCoil, IEnumerable<SlittingJobItemRequest> itemRequests)
    {
        return itemRequests
            .OrderBy(item => item.SequenceNo)
            .Select(item => new SlittingJobItem(
                item.SequenceNo,
                SlitCoilIdGenerator.Generate(motherCoil.RawCoilNumber, item.SequenceNo),
                item.Width,
                SlittingPlanningCalculator.EstimateWeight(item.Width, motherCoil.Thickness, motherCoil.Length),
                item.Remarks))
            .ToArray();
    }

    private static Result ValidateWidth(
        decimal motherCoilWidth,
        IReadOnlyList<SlittingJobItemRequest> items,
        decimal knifeThickness,
        decimal leftEdgeTrim,
        decimal rightEdgeTrim)
    {
        SlittingPlanningSummary summary = SlittingPlanningCalculator.Calculate(
            motherCoilWidth,
            items.Select(item => item.Width),
            knifeThickness,
            leftEdgeTrim,
            rightEdgeTrim);

        return summary.RemainingWidth < 0
            ? Result.Failure(Error.Validation("Allocated width must not exceed mother coil width."))
            : Result.Success();
    }

    private async Task<string> BuildNextJobNumberAsync(CancellationToken cancellationToken)
    {
        int currentYear = DateTime.UtcNow.Year;
        int nextSequence = await slittingJobRepository.CountByYearAsync(currentYear, cancellationToken) + 1;
        string jobNumber;

        do
        {
            jobNumber = SlittingJobNumberGenerator.Generate(currentYear, nextSequence);
            nextSequence++;
        }
        while (await slittingJobRepository.ExistsByJobNumberAsync(jobNumber, cancellationToken));

        return jobNumber;
    }

    private static bool RowVersionMatches(byte[] currentRowVersion, string requestRowVersion)
    {
        try
        {
            byte[] decoded = Convert.FromBase64String(requestRowVersion);
            return currentRowVersion.SequenceEqual(decoded);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static decimal NormalizeParameter(decimal value, decimal fallback)
    {
        return value == 0 ? fallback : value;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
