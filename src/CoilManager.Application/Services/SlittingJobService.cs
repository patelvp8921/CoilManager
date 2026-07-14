using CoilManager.Application.DTOs.SlittingJobs;
using CoilManager.Application.Interfaces.Persistence;
using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Application.Mappings;
using CoilManager.Application.Settings;
using CoilManager.Domain.Entities;
using CoilManager.Domain.Enums;
using CoilManager.Shared.Errors;
using CoilManager.Shared.Pagination;
using CoilManager.Shared.Results;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace CoilManager.Application.Services;

public sealed class SlittingJobService(
    ISlittingJobRepository slittingJobRepository,
    IRawCoilRepository rawCoilRepository,
    ISlitCoilRepository slitCoilRepository,
    IInventoryTransactionRepository inventoryTransactionRepository,
    ICoilNumberingService coilNumberingService,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IValidator<CreateSlittingJobRequest> createValidator,
    IValidator<UpdateSlittingJobRequest> updateValidator,
    IValidator<CompleteSlittingRequest> completeValidator,
    IValidator<StartSlittingRequest> startValidator,
    IOptions<SlittingSettings> slittingOptions,
    IWorkOrderRepository? workOrderRepository = null) : ISlittingJobService
{
    private const decimal DefaultKnifeThickness = 0.2m;
    private const decimal DefaultLeftEdgeTrim = 5m;
    private const decimal DefaultRightEdgeTrim = 5m;
    private readonly SlittingSettings _slittingSettings = slittingOptions.Value;

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
        IReadOnlySet<Guid> draftMotherCoilIds = await slittingJobRepository.GetDraftMotherCoilIdsAsync(cancellationToken);

        return rawCoils
            .Where(rawCoil => rawCoil.Status == CoilStatus.Available)
            .Where(rawCoil => !draftMotherCoilIds.Contains(rawCoil.Id))
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

        if (await slittingJobRepository.DraftExistsForMotherCoilAsync(request.MotherCoilId, cancellationToken))
        {
            return Result<SlittingJobDto>.Failure(Error.Validation("This Mother Coil is already used in a draft slitting job. Release or update the existing draft before creating another job."));
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

        if (request.WorkOrderId.HasValue)
        {
            if (workOrderRepository is null) return Result<SlittingJobDto>.Failure(Error.Validation("Work Order integration is unavailable."));
            WorkOrder? workOrder = await workOrderRepository.GetByIdAsync(request.WorkOrderId.Value, cancellationToken);
            if (workOrder is null) return Result<SlittingJobDto>.Failure(Error.Validation("Work Order was not found."));
            WorkOrderOperation? operation = workOrder.Operations.SingleOrDefault(x => x.OperationType == WorkOrderOperationType.Slitting);
            if (operation is null || !operation.IsRequired || operation.Status != WorkOrderOperationStatus.Pending)
                return Result<SlittingJobDto>.Failure(Error.Validation("Work Order does not have a pending required Slitting operation."));
            if (request.WorkOrderOperationId.HasValue && request.WorkOrderOperationId != operation.Id)
                return Result<SlittingJobDto>.Failure(Error.Validation("Work Order Operation does not match the Slitting operation."));
            job.LinkToWorkOrder(workOrder.Id, workOrder.WorkOrderNumber, operation.Id);
            operation.LinkDocument(job.Id, job.SlittingJobNo);
        }

        job.ReplaceItems(BuildItems(job.SlittingJobNo, motherCoil, request.Items));

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

        IReadOnlyList<SlittingJobItem> rebuiltItems = BuildItems(job.SlittingJobNo, motherCoil, request.Items);

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

        RawCoil? motherCoil = await rawCoilRepository.GetByIdAsync(job.MotherCoilId, cancellationToken);
        if (motherCoil is null)
        {
            return Result<SlittingJobDto>.Failure(Error.Validation("Mother coil is required."));
        }

        if (motherCoil.Status != CoilStatus.Available)
        {
            return Result<SlittingJobDto>.Failure(Error.Validation("Mother Coil must be Available before releasing a slitting job."));
        }

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            DateTimeOffset releasedOn = DateTimeOffset.UtcNow;
            job.Release(CurrentActor(), releasedOn);
            await SyncWorkOrderOperationAsync(job, releasedOn, ct);
            motherCoil.SetStatus(CoilStatus.Reserved);
            await inventoryTransactionRepository.AddAsync(new InventoryTransaction(
                InventoryTransactionType.SlittingJobRelease,
                CoilType.MotherCoil,
                motherCoil.Id,
                motherCoil.RawCoilNumber,
                job.Id,
                job.SlittingJobNo,
                CoilStatus.Available,
                CoilStatus.Reserved,
                motherCoil.Weight,
                releasedOn,
                "Mother Coil reserved for slitting job release."), ct);

            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

        SlittingJob? savedJob = await slittingJobRepository.GetByIdAsync(job.Id, cancellationToken);
        return Result<SlittingJobDto>.Success(SlittingJobDtoMapper.MapToDto(savedJob ?? job));
    }

    public async Task<Result<StartSlittingResponse>> StartSlittingAsync(Guid id, StartSlittingRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        FluentValidation.Results.ValidationResult requestValidation = await startValidator.ValidateAsync(request, cancellationToken);
        if (!requestValidation.IsValid)
        {
            return Result<StartSlittingResponse>.Failure(Error.Validation(string.Join("; ", requestValidation.Errors.Select(error => error.ErrorMessage))));
        }

        SlittingJob? job = await slittingJobRepository.GetByIdAsync(id, cancellationToken);
        if (job is null)
        {
            return Result<StartSlittingResponse>.Failure(Error.NotFound($"Slitting job '{id}' was not found."));
        }

        if (job.Status != SlittingJobStatus.Released)
        {
            return Result<StartSlittingResponse>.Failure(Error.Validation("Only released slitting jobs can be started."));
        }

        if (job.StartedOn.HasValue)
        {
            return Result<StartSlittingResponse>.Failure(Error.Validation("Slitting job is already started."));
        }

        if (!RowVersionMatches(job.RowVersion, request.RowVersion))
        {
            return Result<StartSlittingResponse>.Failure(Error.Conflict("The slitting job was modified by another process. Reload and try again."));
        }

        RawCoil? motherCoil = await rawCoilRepository.GetByIdAsync(job.MotherCoilId, cancellationToken);
        if (motherCoil is null)
        {
            return Result<StartSlittingResponse>.Failure(Error.Validation("Mother coil is required."));
        }

        if (motherCoil.Status != CoilStatus.Reserved)
        {
            return Result<StartSlittingResponse>.Failure(Error.Validation("Mother Coil must be Reserved before starting slitting."));
        }

        DateTimeOffset startedOn = DateTimeOffset.UtcNow;
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            job.Start(CurrentActor(), startedOn, request.MachineId, request.Shift, request.Remarks);
            await SyncWorkOrderOperationAsync(job, startedOn, ct);
            motherCoil.SetStatus(CoilStatus.InProcess);
            await inventoryTransactionRepository.AddAsync(new InventoryTransaction(
                InventoryTransactionType.SlittingStarted,
                CoilType.MotherCoil,
                motherCoil.Id,
                motherCoil.RawCoilNumber,
                job.Id,
                job.SlittingJobNo,
                CoilStatus.Reserved,
                CoilStatus.InProcess,
                motherCoil.Weight,
                startedOn,
                "Mother Coil moved to production for slitting."), ct);

            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

        return Result<StartSlittingResponse>.Success(new(
            job.Id,
            job.SlittingJobNo,
            job.Status,
            motherCoil.RawCoilNumber,
            motherCoil.Status,
            job.StartedBy,
            job.StartedOn ?? startedOn));
    }

    public async Task<Result<SlittingJobDto>> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        SlittingJob? job = await slittingJobRepository.GetByIdAsync(id, cancellationToken);
        if (job is null)
        {
            return Result<SlittingJobDto>.Failure(Error.NotFound($"Slitting job '{id}' was not found."));
        }

        if (job.Status == SlittingJobStatus.InProgress)
        {
            return Result<SlittingJobDto>.Failure(Error.Validation("In progress slitting jobs cannot be cancelled in MVP. Pause/abort workflow is deferred."));
        }

        if (job.Status != SlittingJobStatus.Released)
        {
            return Result<SlittingJobDto>.Failure(Error.Validation("Only released slitting jobs can be cancelled."));
        }

        RawCoil? motherCoil = await rawCoilRepository.GetByIdAsync(job.MotherCoilId, cancellationToken);
        if (motherCoil is null)
        {
            return Result<SlittingJobDto>.Failure(Error.Validation("Mother coil is required."));
        }

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            DateTimeOffset cancelledOn = DateTimeOffset.UtcNow;
            job.Cancel(CurrentActor(), cancelledOn);
            motherCoil.SetStatus(CoilStatus.Available);
            await inventoryTransactionRepository.AddAsync(new InventoryTransaction(
                InventoryTransactionType.SlittingJobCancel,
                CoilType.MotherCoil,
                motherCoil.Id,
                motherCoil.RawCoilNumber,
                job.Id,
                job.SlittingJobNo,
                CoilStatus.Reserved,
                CoilStatus.Available,
                motherCoil.Weight,
                cancelledOn,
                "Released slitting job cancelled."), ct);

            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

        SlittingJob? savedJob = await slittingJobRepository.GetByIdAsync(job.Id, cancellationToken);
        return Result<SlittingJobDto>.Success(SlittingJobDtoMapper.MapToDto(savedJob ?? job));
    }

    public async Task<Result<CompleteSlittingResponse>> CompleteAsync(Guid id, CompleteSlittingRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        FluentValidation.Results.ValidationResult requestValidation = await completeValidator.ValidateAsync(request, cancellationToken);
        if (!requestValidation.IsValid)
        {
            return Result<CompleteSlittingResponse>.Failure(Error.Validation(string.Join("; ", requestValidation.Errors.Select(error => error.ErrorMessage))));
        }

        SlittingJob? job = await slittingJobRepository.GetByIdAsync(id, cancellationToken);
        if (job is null)
        {
            return Result<CompleteSlittingResponse>.Failure(Error.NotFound($"Slitting job '{id}' was not found."));
        }

        RawCoil? motherCoil = await rawCoilRepository.GetByIdAsync(job.MotherCoilId, cancellationToken);
        if (motherCoil is null)
        {
            return Result<CompleteSlittingResponse>.Failure(Error.Validation("Mother coil is required."));
        }

        Result validationResult = await ValidateCompletionAsync(job, motherCoil, request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result<CompleteSlittingResponse>.Failure(validationResult.Error);
        }

        Dictionary<Guid, CompleteSlittingItemRequest> actuals = request.Slits.ToDictionary(item => item.SlittingJobItemId);
        List<SlitCoil> generatedCoils = [];
        DateTimeOffset completedOn = DateTimeOffset.UtcNow;

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            foreach (SlittingJobItem item in job.Items.OrderBy(item => item.SequenceNo))
            {
                CompleteSlittingItemRequest actual = actuals[item.Id];
                decimal actualWidth = actual.ActualWidth ?? item.Width;
                string coilNumber = coilNumberingService.GenerateFirstGenerationSlitCoilNumber(motherCoil.RawCoilNumber, item.SequenceNo);

                item.Complete(actualWidth, actual.ActualWeight, actual.Remarks);

                SlitCoil slitCoil = new(
                    coilNumber,
                    motherCoil.Id,
                    motherCoil.Id,
                    motherCoil.Id,
                    job.Id,
                    item.SequenceNo,
                    1,
                    motherCoil.GradeId,
                    motherCoil.SupplierId,
                    motherCoil.ManufacturerId,
                    motherCoil.HeatNumber,
                    motherCoil.Thickness,
                    motherCoil.Category,
                    motherCoil.CoreLossPerKg,
                    actualWidth,
                    actual.ActualWeight,
                    motherCoil.WarehouseLocation,
                    _slittingSettings.DefaultLabelVersion);

                generatedCoils.Add(slitCoil);
                await slitCoilRepository.AddAsync(slitCoil, ct);
                await inventoryTransactionRepository.AddAsync(new InventoryTransaction(
                    InventoryTransactionType.SlitCoilGeneration,
                    CoilType.SlitCoil,
                    slitCoil.Id,
                    slitCoil.CoilNumber,
                    job.Id,
                    job.SlittingJobNo,
                    null,
                    CoilStatus.Available,
                    slitCoil.Weight,
                    completedOn,
                    "Slit Coil generated from completed slitting job."), ct);
            }

            job.Complete(CurrentActor(), completedOn);
            await SyncWorkOrderOperationAsync(job, completedOn, ct);
            motherCoil.SetStatus(CoilStatus.Consumed);
            await inventoryTransactionRepository.AddAsync(new InventoryTransaction(
                InventoryTransactionType.SlittingJobComplete,
                CoilType.MotherCoil,
                motherCoil.Id,
                motherCoil.RawCoilNumber,
                job.Id,
                job.SlittingJobNo,
                CoilStatus.InProcess,
                CoilStatus.Consumed,
                motherCoil.Weight,
                completedOn,
                "Mother Coil consumed by completed slitting job."), ct);

            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

        IReadOnlyList<SlitCoil> savedCoils = await slitCoilRepository.GetBySlittingJobIdAsync(job.Id, cancellationToken);
        decimal remainingWidth = CalculateRemainingWidth(job, motherCoil);
        decimal unusedEstimatedWeight = remainingWidth > 0
            ? SlittingPlanningCalculator.EstimateWeight(remainingWidth, motherCoil.Width, motherCoil.Weight, motherCoil.Thickness, motherCoil.Length)
            : 0;
        List<string> warnings = [];
        if (remainingWidth > _slittingSettings.MinimumBalanceWidthMm)
        {
            warnings.Add("Remaining material detected. Balance Coil creation is not included in MVP.");
        }

        return Result<CompleteSlittingResponse>.Success(new(
            job.Id,
            job.SlittingJobNo,
            motherCoil.RawCoilNumber,
            savedCoils.Select(SlitCoilDtoMapper.MapToGeneratedDto).ToArray(),
            savedCoils.Sum(coil => coil.Weight),
            completedOn,
            remainingWidth,
            unusedEstimatedWeight,
            warnings));
    }

    public async Task<Result<SlittingJobCompletionDto>> GetCompletionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        SlittingJob? job = await slittingJobRepository.GetByIdAsync(id, cancellationToken);
        if (job is null)
        {
            return Result<SlittingJobCompletionDto>.Failure(Error.NotFound($"Slitting job '{id}' was not found."));
        }

        IReadOnlyList<SlitCoil> coils = await slitCoilRepository.GetBySlittingJobIdAsync(id, cancellationToken);
        return Result<SlittingJobCompletionDto>.Success(new(
            job.Id,
            job.SlittingJobNo,
            job.Status,
            job.MotherCoil?.RawCoilNumber ?? "-",
            coils.Sum(coil => coil.Weight),
            coils.Select(SlitCoilDtoMapper.MapToGeneratedDto).ToArray()));
    }

    private IReadOnlyList<SlittingJobItem> BuildItems(string slittingJobNo, RawCoil motherCoil, IEnumerable<SlittingJobItemRequest> itemRequests)
    {
        return itemRequests
            .OrderBy(item => item.SequenceNo)
            .Select(item => new SlittingJobItem(
                item.SequenceNo,
                coilNumberingService.GenerateFirstGenerationSlitCoilNumber(motherCoil.RawCoilNumber, item.SequenceNo),
                item.Width,
                SlittingPlanningCalculator.EstimateWeight(item.Width, motherCoil.Width, motherCoil.Weight, motherCoil.Thickness, motherCoil.Length),
                item.Remarks))
            .ToArray();
    }

    private async Task<Result> ValidateCompletionAsync(
        SlittingJob job,
        RawCoil motherCoil,
        CompleteSlittingRequest request,
        CancellationToken cancellationToken)
    {
        if (job.Status == SlittingJobStatus.Completed)
        {
            return Result.Failure(Error.Validation("Slitting job is already completed."));
        }

        if (job.Status != SlittingJobStatus.InProgress)
        {
            return Result.Failure(Error.Validation("Only in progress slitting jobs can be completed."));
        }

        if (motherCoil.Status != CoilStatus.InProcess)
        {
            return Result.Failure(Error.Validation("Mother Coil must be In Process before completing a slitting job."));
        }

        if (!RowVersionMatches(job.RowVersion, request.RowVersion))
        {
            return Result.Failure(Error.Conflict("The slitting job was modified by another process. Reload and try again."));
        }

        if (request.Slits.Count == 0)
        {
            return Result.Failure(Error.Validation("Complete slitting request must include actual slit details."));
        }

        Guid[] plannedItemIds = job.Items.Select(item => item.Id).OrderBy(id => id).ToArray();
        Guid[] requestItemIds = request.Slits.Select(item => item.SlittingJobItemId).OrderBy(id => id).ToArray();
        if (!plannedItemIds.SequenceEqual(requestItemIds) || request.Slits.Select(item => item.SlittingJobItemId).Distinct().Count() != request.Slits.Count)
        {
            return Result.Failure(Error.Validation("Every planned slit must appear exactly once."));
        }

        Dictionary<Guid, SlittingJobItem> plannedItems = job.Items.ToDictionary(item => item.Id);
        decimal totalActualWeight = 0;
        foreach (CompleteSlittingItemRequest slit in request.Slits)
        {
            if (slit.ActualWeight <= 0)
            {
                return Result.Failure(Error.Validation("Every planned slit must have actual weight greater than zero."));
            }

            SlittingJobItem plannedItem = plannedItems[slit.SlittingJobItemId];
            decimal actualWidth = slit.ActualWidth ?? plannedItem.Width;
            if (actualWidth <= 0)
            {
                return Result.Failure(Error.Validation("Actual width must be greater than zero."));
            }

            if (Math.Abs(actualWidth - plannedItem.Width) > _slittingSettings.WidthToleranceMm)
            {
                return Result.Failure(Error.Validation($"Actual width for slit {plannedItem.SequenceNo} exceeds configured tolerance."));
            }

            totalActualWeight += slit.ActualWeight;
        }

        if (totalActualWeight > motherCoil.Weight + _slittingSettings.WeightToleranceKg)
        {
            return Result.Failure(Error.Validation("Total actual slit weight must not exceed Mother Coil weight plus tolerance."));
        }

        foreach (SlittingJobItem item in job.Items)
        {
            string coilNumber = coilNumberingService.GenerateFirstGenerationSlitCoilNumber(motherCoil.RawCoilNumber, item.SequenceNo);
            if (await slitCoilRepository.ExistsByCoilNumberAsync(coilNumber, cancellationToken))
            {
                return Result.Failure(Error.Conflict($"Slit Coil number '{coilNumber}' already exists."));
            }
        }

        return Result.Success();
    }

    private static decimal CalculateRemainingWidth(SlittingJob job, RawCoil motherCoil)
    {
        SlittingPlanningSummary summary = SlittingPlanningCalculator.Calculate(
            motherCoil.Width,
            job.Items.Select(item => item.ActualWidth ?? item.Width),
            job.KnifeThickness,
            job.LeftEdgeTrim,
            job.RightEdgeTrim);

        return Math.Max(summary.RemainingWidth, 0);
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

    private string CurrentActor()
    {
        return Normalize(currentUserService.UserName)
            ?? Normalize(currentUserService.UserId)
            ?? "System";
    }

    private async Task SyncWorkOrderOperationAsync(SlittingJob job, DateTimeOffset at, CancellationToken token)
    {
        if (!job.WorkOrderId.HasValue || workOrderRepository is null) return;
        WorkOrder? workOrder = await workOrderRepository.GetByIdAsync(job.WorkOrderId.Value, token);
        WorkOrderOperation? operation = workOrder?.Operations.SingleOrDefault(x => x.Id == job.WorkOrderOperationId);
        operation?.SynchronizeSlittingJob(job.Status, at);
    }
}
